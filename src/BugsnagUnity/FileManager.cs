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

        private static List<SessionReport> _pendingSessions = new List<SessionReport>();

        private static List<Report> _pendingEvents = new List<Report>();

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
            File.WriteAllText(_deviceIdFile, json);
        }

        internal static void InitFileManager(Configuration configuration)
        {
            _configuration = configuration;
            CheckForDirectoryCreation();
        }

        internal static void AddPendingSession(SessionReport sessionReport)
        {
            _pendingSessions.Add(sessionReport);
        }

        internal static void AddPendingEvent(Report report)
        {
            RemovePendingEvent(report.Id);
            _pendingEvents.Add(report);
        }

        private static SessionReport GetPendingSessionReport(string id)
        {
            foreach (var report in _pendingSessions)
            {
                if (report.Id.Equals(id))
                {
                    return report;
                }
            }
            return null;
        }

        private static Report GetPendingEventReport(string id)
        {
            foreach (var report in _pendingEvents)
            {
                if (report.Id.Equals(id))
                {
                    return report;
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

        private static bool SessionAlreadyCached(string payloadId)
        {
            foreach (var cachedSession in _cachedSessions)
            {
                if (cachedSession.Contains(payloadId))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void CacheSession(string reportId)
        {
            if (SessionAlreadyCached(reportId))
            {
                return;
            }
            var sessionReport = GetPendingSessionReport(reportId);
            if (sessionReport == null)
            {
                return;
            }
            var path = _sessionsDirectory + "/" + sessionReport.Id + ".session";
            var data = sessionReport.GetSerialisableSessionReport();
            WriteToDisk(data, path);
            CheckForMaxCachedSessions();
        }

        internal static void CacheEvent(string reportId)
        {
            var eventReport = GetPendingEventReport(reportId);
            if (eventReport == null)
            {
                return;
            }
            var path = _eventsDirectory + "/" + eventReport.Id + ".event";
            var data = eventReport.GetSerialisableEventReport();
            WriteToDisk(data,path);
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
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    RemovePendingSession(payload.Id);
                    RemoveCachedSession(payload.Id);
                    break;
                case PayloadType.Event:
                    RemoveCachedEvent(payload.Id);
                    RemovePendingEvent(payload.Id);
                    break;
            }
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

        private static void RemovePendingEvent(string id)
        {
            _pendingEvents.RemoveAll(item => item.Id == id);
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

        private static void RemovePendingSession(string id)
        {
            _pendingSessions.RemoveAll(item => item.Id == id);
        }

        internal static List<IPayload> GetCachedPayloads()
        {
            var cachedPayloads = new List<IPayload>();

            //Get cached sessions
            foreach (var cachedSessionPath in _cachedSessions)
            {
                var json = File.ReadAllText(cachedSessionPath);
                var sessionReportFromCachedPayload = new SessionReport(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                cachedPayloads.Add(sessionReportFromCachedPayload);
            }

            //get cached events and run them through on send callbacks
            var cachedEvents = _cachedEvents.ToList();
            foreach (var cachedEventPath in cachedEvents)
            {
                var json = File.ReadAllText(cachedEventPath);
                var eventReportFromCachedPayload = new Report(_configuration, ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary());
                var shouldDiscard = false;
                foreach (var onSendErrorCallback in _configuration.GetOnSendErrorCallbacks())
                {
                    try
                    {
                        if (!onSendErrorCallback.Invoke(eventReportFromCachedPayload.Event))
                        {
                            shouldDiscard = true;
                            break;
                        }
                    }
                    catch
                    {
                        // If the callback causes an exception, ignore it and execute the next one
                    }
                }
                if (shouldDiscard)
                {
                    RemoveCachedEvent(eventReportFromCachedPayload.Id);
                    RemovePendingEvent(eventReportFromCachedPayload.Id);
                }
                else
                {
                    AddPendingEvent(eventReportFromCachedPayload);
                    eventReportFromCachedPayload.ApplyEventPayload();
                    cachedPayloads.Add(eventReportFromCachedPayload);
                }
            }
            return cachedPayloads;
        }

        private static void WriteToDisk(Dictionary<string,object> data, string path)
        {
            CheckForDirectoryCreation();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using var fileStream = File.Create(path);
            using var writer = new StreamWriter(fileStream, new UTF8Encoding(false)) { AutoFlush = false };
            SimpleJson.SerializeObject(data, writer);
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
