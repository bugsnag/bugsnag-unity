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

        private const string SESSION_FILE_PREFIX = ".session";
        private const string EVENT_FILE_PREFIX = ".event";


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

        private static string[] _cachedSessions => Directory.GetFiles(_sessionsDirectory, "*" + SESSION_FILE_PREFIX);

        private static string[] _cachedEvents => Directory.GetFiles(_eventsDirectory, "*" + EVENT_FILE_PREFIX);

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
            var path = _sessionsDirectory + "/" + pendingSession.PayloadId + SESSION_FILE_PREFIX;
            WritePayloadToDisk(pendingSession.Json, path);
            CheckForMaxCachedPayloads(_cachedSessions, _configuration.MaxPersistedSessions);
        }

        internal static void CacheEvent(string reportId)
        {
            var eventReport = GetPendingPayload(reportId);
            if (eventReport == null)
            {
                return;
            }
            var path = _eventsDirectory + "/" + eventReport.PayloadId + EVENT_FILE_PREFIX;
            WritePayloadToDisk(eventReport.Json, path);
            RemovePendingPayload(reportId);
            CheckForMaxCachedPayloads(_cachedEvents, _configuration.MaxPersistedEvents);
        }

        private static void CheckForMaxCachedPayloads(string[] payloads, int maxPayloads)
        {
            if (payloads.Length > maxPayloads)
            {
                RemoveOldestFiles(payloads, payloads.Length - maxPayloads);
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

        internal static string[] GetCachedPayloadPaths()
        {
            var cachedPayloadPaths = new List<string>();
            cachedPayloadPaths.AddRange(_cachedSessions);
            cachedPayloadPaths.AddRange(_cachedEvents);
            return cachedPayloadPaths.ToArray();
        }

        internal static IPayload GetPayloadFromCachePath(string path)
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
