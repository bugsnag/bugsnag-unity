using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BugsnagUnity.Payload;
using UnityEngine;
namespace BugsnagUnity
{
    public class FileManager
    {

        private const int MAX_CACHED_DAYS = 60;

        private static List<PendingPayload> _pendingPayloads = new List<PendingPayload>();

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

        private static string[] _cachedSessions => Directory.GetFiles(_sessionsDirectory, "*.session");

        private static string[] _cachedEvents => Directory.GetFiles(_eventsDirectory, "*.event");

        private static string _deviceIdFile = _cacheDirectory + "/deviceId.txt";

        private class PendingPayload
        {
            public string Json;
            public string PayloadId;

            public PendingPayload(string json, string payloadId)
            {
                Json = json;
                PayloadId = payloadId;
            }
        }

        #region Device Id Persistence

        [Serializable]
        private class DeviceIdModel
        {
            public string DeviceId;
        }

        internal static string GetDeviceId()
        {
            try
            {
                var deviceId = string.Empty;
                if (File.Exists(_deviceIdFile))
                {

                    var deviceIdStore = JsonUtility.FromJson<DeviceIdModel>(File.ReadAllText(_deviceIdFile));
                    deviceId = deviceIdStore.DeviceId;
                }
                if (string.IsNullOrEmpty(deviceId) && _configuration.GenerateAnonymousId)
                {
                    deviceId = Guid.NewGuid().ToString();
                    StoreDeviceId(deviceId);
                }
                return deviceId;
            }
            catch
            {
                // not possible in unit tests
                return string.Empty;
            }
           
        }

        private static void StoreDeviceId(string deviceId)
        {
            var model = new DeviceIdModel()
            {
                DeviceId = deviceId
            };
            var json = JsonUtility.ToJson(model);
            CheckForDirectoryCreation();
            File.WriteAllText(_deviceIdFile, json);
        }

        #endregion

        internal static void InitFileManager(Configuration configuration)
        {
            _configuration = configuration;
            CheckForDirectoryCreation();
            RemoveExpiredPayloads();
        }

        private static void RemoveExpiredPayloads()
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
                        Debug.LogWarning("Bugsnag Warning: Discarding historic event from " + creationTime.ToLongDateString() + " after failed delivery");
                        File.Delete(file);
                    }
                }
            }
            catch { }
           
        }

        internal static void AddPendingPayload(IPayload payload)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(payload.GetSerialisablePayload(), writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                _pendingPayloads.Add(new PendingPayload(jsonString,payload.Id));
            }
        }

        private static PendingPayload GetPendingPayload(string id)
        {
            foreach (var payload in _pendingPayloads)
            {
                if (payload.PayloadId == id)
                {
                    return payload;
                }
            }
            return null;
        }

        internal static void SendPayloadFailed(IPayload payload)
        {
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    CacheSession(payload.Id);
                    break;
                case PayloadType.Event:
                    CacheEvent(payload.Id);
                    break;
            }
        }

        internal static void CacheSession(string reportId)
        {
            var pendingSession = GetPendingPayload(reportId);
            if (pendingSession == null)
            {
                return;
            }
            var path = _sessionsDirectory + "/" + pendingSession.PayloadId + ".session";
            WritePayloadToDisk(pendingSession.Json, path);
            CheckForMaxCachedSessions();
        }

        internal static void CacheEvent(string reportId)
        {
            var eventReport = GetPendingPayload(reportId);
            if (eventReport == null)
            {
                return;
            }
            var path = _eventsDirectory + "/" + eventReport.PayloadId + ".event";
            WritePayloadToDisk(eventReport.Json, path);
            RemovePendingPayload(reportId);
            CheckForMaxCachedEvents();          
        }

        private static void CheckForMaxCachedEvents()
        {
            var filesCount = _cachedEvents.Length;
            while (filesCount > _configuration.MaxPersistedEvents)
            {
                RemoveOldestFiles(_cachedEvents, filesCount - _configuration.MaxPersistedEvents);
                filesCount = _cachedEvents.Length;
            }
        }

        private static void CheckForMaxCachedSessions()
        {
            var filesCount = _cachedSessions.Length;
            while(filesCount > _configuration.MaxPersistedSessions)
            {
                RemoveOldestFiles(_cachedSessions, filesCount - _configuration.MaxPersistedSessions);
                filesCount = _cachedSessions.Length;
            }
        }

        private static void RemoveOldestFiles(string[] filePaths,int numToRemove)
        {
            var ordered = filePaths.OrderBy(file => File.GetCreationTimeUtc(file)).ToArray();
            foreach (var file in ordered.Take(numToRemove))
            {
                File.Delete(file);
            }
        }

        internal static void PayloadSendSuccess(IPayload payload)
        {
            RemovePendingPayload(payload.Id);
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    RemoveCachedSession(payload.Id);
                    break;
                case PayloadType.Event:
                    RemoveCachedEvent(payload.Id);
                    break;
            }
        }

        private static void RemovePendingPayload(string id)
        {
            _pendingPayloads.RemoveAll(item => item.PayloadId == id);
        }

        internal static void RemoveCachedEvent(string id)
        {
            foreach (var cachedEventPath in _cachedEvents)
            {
                if (cachedEventPath.Contains(id))
                {
                    File.Delete(cachedEventPath);
                }
            }
        }

        internal static void RemoveCachedSession(string id)
        {
            foreach (var cachedSessionPath in _cachedSessions)
            {
                if (cachedSessionPath.Contains(id))
                {
                    File.Delete(cachedSessionPath);
                }
            }
        }

        internal static List<IPayload> GetCachedPayloads()
        {
            var cachedPayloads = new List<IPayload>();

            foreach (var cachedSessionPath in _cachedSessions)
            {
                var json = File.ReadAllText(cachedSessionPath);
                var sessionReportFromCachedPayload = new SessionReport(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                cachedPayloads.Add(sessionReportFromCachedPayload);
            }

            var cachedEvents = _cachedEvents.ToList();
            foreach (var cachedEventPath in cachedEvents)
            {
                var json = File.ReadAllText(cachedEventPath);
                var eventReportFromCachedPayload = new Report(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                cachedPayloads.Add(eventReportFromCachedPayload);
            }
            return cachedPayloads;
        }

        private static void WritePayloadToDisk(string jsonData, string path)
        {
            CheckForDirectoryCreation();
            File.WriteAllText(path, jsonData);
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
