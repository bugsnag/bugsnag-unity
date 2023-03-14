﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnity
{
    internal class CacheManager : ICacheManager
    {

        private static Configuration _configuration;

        private static string _cacheDirectory
        {
            get { return Application.persistentDataPath + "/Bugsnag"; }
        }

        private static string _sessionsDirectory
        {
            get { return _cacheDirectory + "/Sessions"; }
        }

        private static string _eventsDirectory
        {
            get { return _cacheDirectory + "/Events"; }
        }

        private static string _deviceIdFile = _cacheDirectory + "/deviceId.txt";

        private const string SESSION_FILE_SUFFIX = ".session";

        private const string EVENT_FILE_SUFFIX = ".event";

        private const int MAX_CACHED_DAYS = 60;



        public CacheManager(Configuration configuration)
        {
            _configuration = configuration;
            CheckForDirectoryCreation();
            RemoveExpiredPayloads();
        }

        private string[] GetCachedEventFiles()
        {
            return GetFilesBySuffix(_eventsDirectory, EVENT_FILE_SUFFIX);
        }

        private string[] GetCachedSessionFiles()
        {

            return GetFilesBySuffix(_sessionsDirectory,SESSION_FILE_SUFFIX);
        }

        private string[] GetFilesBySuffix(string path, string suffix)
        {
            try
            {
                var files = Directory.GetFiles(path, "*" + suffix);
                return files;
            }
            catch
            {
                return new string[] { };
            }
        }

        public string GetCachedDeviceId()
        {

            if (!_configuration.GenerateAnonymousId)
            {
                return string.Empty;
            }
            try
            {
                if (File.Exists(_deviceIdFile))
                {
                    // return existing cached device id
                    return File.ReadAllText(_deviceIdFile);
                }

                // create and cache new random device id
                var newDeviceId = Guid.NewGuid().ToString();
                SaveDeviceIdToCache(newDeviceId);
                return newDeviceId;
            }
            catch
            {
                // not possible in unit tests
                return string.Empty;
            }

        }

        public void SaveDeviceIdToCache(string deviceId)
        {
            WriteFile(_deviceIdFile, deviceId);
        }

        private void RemoveExpiredPayloads()
        {
            var files = GetCachedEventFiles().ToList();
            files.AddRange(GetCachedSessionFiles());
            foreach (var file in files)
            {
                try
                {
                    var creationTime = File.GetCreationTimeUtc(file);
                    if ((DateTime.UtcNow - creationTime).TotalDays > MAX_CACHED_DAYS)
                    {
                        Debug.LogWarning("Bugsnag Warning: Discarding historical event from " + creationTime.ToLongDateString() + " after failed delivery");
                        File.Delete(file);
                    }
                }
                catch { }
            }
        }

        public void SaveSessionToCache(string id,string json)
        {
            var path = _sessionsDirectory + "/" + id + SESSION_FILE_SUFFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(GetCachedSessionFiles(), _configuration.MaxPersistedSessions);
        }

        public void SaveEventToCache(string id, string json)
        {
            var path = _eventsDirectory + "/" + id + EVENT_FILE_SUFFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(GetCachedEventFiles(), _configuration.MaxPersistedEvents);
        }

        private void WritePayloadToDisk(string jsonData, string path)
        {
            WriteFile(path, jsonData);
        }

        private void CheckForMaxCachedPayloads(string[] payloads, int maxPayloads)
        {
            if (payloads.Length > maxPayloads)
            {
                RemoveOldestFiles(payloads, payloads.Length - maxPayloads);
            }
        }

        private void RemoveOldestFiles(string[] filePaths, int numToRemove)
        {
            var ordered = filePaths.OrderBy(file => File.GetCreationTimeUtc(file)).ToArray();
            foreach (var file in ordered.Take(numToRemove))
            {
                DeleteFile(file);
            }            
        }

        public void RemoveCachedEvent(string id)
        {
            RemovePayloadWithID(GetCachedEventFiles(), id);
        }

        public void RemoveCachedSession(string id)
        {
            RemovePayloadWithID(GetCachedSessionFiles(), id);
        }

        private void RemovePayloadWithID(string[] files, string id)
        {
            foreach (var path in files)
            {
                if (path.Contains(id))
                {
                    DeleteFile(path);
                    return;
                }
            }
        }

        private void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }catch{}
        }

        private void WriteFile(string path, string data)
        {
            try
            {
                File.WriteAllText(path, data);
            }catch { }
        }

        private string GetJsonFromCachePath(string path)
        {
            try {
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            } catch { }
            return null;
        }
       
        private static void CheckForDirectoryCreation()
        {
            try
            {
                if (!Directory.Exists(_cacheDirectory))
                {
                    Directory.CreateDirectory(_cacheDirectory);
                }
                if (!Directory.Exists(_sessionsDirectory))
                {
                    Directory.CreateDirectory(_sessionsDirectory);
                }
                if (!Directory.Exists(_eventsDirectory))
                {
                    Directory.CreateDirectory(_eventsDirectory);
                }
            }
            catch
            {
                //not possible in unit tests
            }

        }

        public List<string> GetCachedEventIds()
        {
            return GetPayloadIDsFromDirectory(GetCachedEventFiles());
        }

        public List<string> GetCachedSessionIds()
        {
            return GetPayloadIDsFromDirectory(GetCachedSessionFiles());
        }

        private List<string> GetPayloadIDsFromDirectory(string[] files)
        {
            var names = new List<string>();
            var ordered = GetCreationOrderedFiles(files);
            foreach (var path in ordered)
            {
                try
                {
                    names.Add(Path.GetFileNameWithoutExtension(path));
                }
                catch { }
            }
            return names;
        }

        private string[] GetCreationOrderedFiles(string[] files)
        {
            try
            {
                var orderedFiles = files.OrderBy(file => File.GetCreationTimeUtc(file)).ToArray();
                return orderedFiles;
            }
            catch
            {
                return new string[] { };
            }
        }

        public string GetCachedEvent(string id)
        {
            foreach (var path in GetCachedEventFiles())
            {
                if (path.Contains(id))
                {
                    return GetJsonFromCachePath(path);
                }
            }
            return null;
        }

        public string GetCachedSession(string id)
        {
            foreach (var path in GetCachedSessionFiles())
            {
                if (path.Contains(id))
                {
                    return GetJsonFromCachePath(path);
                }
            }
            return null;
        }
    }
}
