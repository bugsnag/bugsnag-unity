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

        private static string[] _cachedSessions => Directory.GetFiles(_sessionsDirectory, "*" + SESSION_FILE_PREFIX);

        private static string[] _cachedEvents => Directory.GetFiles(_eventsDirectory, "*" + EVENT_FILE_PREFIX);

        private static string _deviceIdFile = _cacheDirectory + "/deviceId.txt";

        private const string SESSION_FILE_PREFIX = ".session";

        private const string EVENT_FILE_PREFIX = ".event";

        private const int MAX_CACHED_DAYS = 60;


        public CacheManager(Configuration configuration)
        {
            _configuration = configuration;
            CheckForDirectoryCreation();
            try
            {
                RemoveExpiredPayloads();
            }
            catch
            {
                //not avaliable in unit tests
            }
        }

        public string GetCachedDeviceId()
        {
            try
            {
                var deviceId = string.Empty;
                if (File.Exists(_deviceIdFile))
                {
                    deviceId = File.ReadAllText(_deviceIdFile);
                }
                if (string.IsNullOrEmpty(deviceId) && _configuration.GenerateAnonymousId)
                {
                    deviceId = Guid.NewGuid().ToString();
                    SaveDeviceIdToCache(deviceId);
                }
                return deviceId;
            }
            catch
            {
                // not possible in unit tests
                return string.Empty;
            }

        }

        public void SaveDeviceIdToCache(string deviceId)
        {
            try
            {
                File.WriteAllText(_deviceIdFile, deviceId);
            }
            catch { }
        }

        private void RemoveExpiredPayloads()
        {
            var files = _cachedEvents.ToList();
            files.AddRange(_cachedSessions);
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
            var path = _sessionsDirectory + "/" + id + SESSION_FILE_PREFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(_cachedSessions, _configuration.MaxPersistedSessions);
        }

        public void SaveEventToCache(string id, string json)
        {
            var path = _eventsDirectory + "/" + id + EVENT_FILE_PREFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(_cachedEvents, _configuration.MaxPersistedEvents);
        }

        private void WritePayloadToDisk(string jsonData, string path)
        {
            try
            {
                File.WriteAllText(path, jsonData);
            }
            catch{ }
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
                try
                {
                    File.Delete(file);
                }
                catch { }
            }            
        }

        public void RemoveCachedEvent(string id)
        {
            try
            {
                foreach (var cachedEventPath in _cachedEvents)
                {
                    if (cachedEventPath.Contains(id))
                    {
                        File.Delete(cachedEventPath);
                    }
                }
            }
            catch { }
        }

        public void RemoveCachedSession(string id)
        {
            try
            {
                foreach (var cachedSessionPath in _cachedSessions)
                {
                    if (cachedSessionPath.Contains(id))
                    {
                        File.Delete(cachedSessionPath);
                    }
                }
            }
            catch { }
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
            return GetPayloadIDsFromDirectory(_cachedEvents);
        }

        public List<string> GetCachedSessionIds()
        {
            return GetPayloadIDsFromDirectory(_cachedSessions);
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
            foreach (var path in _cachedEvents)
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
            foreach (var path in _cachedSessions)
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
