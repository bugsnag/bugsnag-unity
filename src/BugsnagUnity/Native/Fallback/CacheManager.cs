using System;
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
            RemoveExpiredPayloads();
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
            File.WriteAllText(_deviceIdFile, deviceId);
        }

        private void RemoveExpiredPayloads()
        {
            try
            {
                var files = _cachedEvents.ToList();
                files.AddRange(_cachedSessions);
                foreach (var file in files)
                {
                    var creationTime = File.GetCreationTimeUtc(file);
                    if ((DateTime.UtcNow - creationTime).TotalDays > MAX_CACHED_DAYS)
                    {
                        Debug.LogWarning("Bugsnag Warning: Discarding historical event from " + creationTime.ToLongDateString() + " after failed delivery");
                        File.Delete(file);
                    }
                }
            }
            catch { }
        }

        public void CacheSession(string id,string json)
        {
            var path = _sessionsDirectory + "/" + id + SESSION_FILE_PREFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(_cachedSessions, _configuration.MaxPersistedSessions);
        }

        public void CacheEvent(string id, string json)
        {
            var path = _eventsDirectory + "/" + id + EVENT_FILE_PREFIX;
            WritePayloadToDisk(json, path);
            CheckForMaxCachedPayloads(_cachedEvents, _configuration.MaxPersistedEvents);
        }

        private void WritePayloadToDisk(string jsonData, string path)
        {
            File.WriteAllText(path, jsonData);
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
                File.Delete(file);
            }
        }

        public void RemoveCachedEvent(string id)
        {
            foreach (var cachedEventPath in _cachedEvents)
            {
                if (cachedEventPath.Contains(id))
                {
                    File.Delete(cachedEventPath);
                }
            }
        }

        public void RemoveCachedSession(string id)
        {
            foreach (var cachedSessionPath in _cachedSessions)
            {
                if (cachedSessionPath.Contains(id))
                {
                    File.Delete(cachedSessionPath);
                }
            }
        }

        public string[] GetCachedPayloadIds()
        {
            var cachedPayloadIds = new List<string>();
            foreach (var path in _cachedSessions)
            {
                cachedPayloadIds.Add(Path.GetFileNameWithoutExtension(path));
            }
            foreach (var path in _cachedEvents)
            {
                cachedPayloadIds.Add(Path.GetFileNameWithoutExtension(path));
            }
            return cachedPayloadIds.ToArray();
        }

        private string[] GetCachedPayloadPaths()
        {
            var cachedPayloadPaths = new List<string>();
            cachedPayloadPaths.AddRange(_cachedSessions);
            cachedPayloadPaths.AddRange(_cachedEvents);
            return cachedPayloadPaths.OrderBy(file => File.GetCreationTimeUtc(file)).ToArray();
        }

        private IPayload GetPayloadFromCachePath(string path)
        {
            if (File.Exists(path))
            {
                if (path.EndsWith(SESSION_FILE_PREFIX))
                {
                    var json = File.ReadAllText(path);
                    var sessionReportFromCachedPayload = new SessionReport(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                    return sessionReportFromCachedPayload;
                }
                else if (path.EndsWith(EVENT_FILE_PREFIX))
                {
                    var json = File.ReadAllText(path);
                    var eventReportFromCachedPayload = new Report(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                    return eventReportFromCachedPayload;
                }
            }
            return null;
        }

        public IPayload GetCachedPayload(string id)
        {
            foreach (var path in GetCachedPayloadPaths())
            {
                return GetPayloadFromCachePath(path);
            }
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

    }
}
