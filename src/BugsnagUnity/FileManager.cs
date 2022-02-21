﻿using System;
using System.Collections.Generic;
using System.IO;
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

        private static string CacheDirectory
        {
            get { return Application.persistentDataPath + "/Bugsnag"; }
        }

        private static string SessionsDirectory
        {
            get { return CacheDirectory + "/Sessions"; }
        }

        private static string EventsDirectory
        {
            get { return CacheDirectory + "/Events"; }
        }

        private static string[] _cachedSessions => Directory.GetFiles(SessionsDirectory, "*.session");

        private static string[] _cachedEvents => Directory.GetFiles(EventsDirectory, "*.event");


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
            if (!PayloadAlreadyCached(payload.Id))
            {
                switch (payload.PayloadType)
                {
                    case PayloadType.Session:
                        CacheSession(payload.Id);
                        break;
                    case PayloadType.Event:
                        break;
                }
            }
        }

        private static bool PayloadAlreadyCached(string payloadId)
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
            var sessionReport = GetPendingSessionReport(reportId);
            if (sessionReport == null)
            {
                return;
            }
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                // Use a dictionary that breaks out the session from the sessions array due to issues deserialising that array later
                var serialisableSessionReport = new Dictionary<string, object>
                {
                    { "payloadId", sessionReport.Id },
                    { "app", sessionReport["app"] },
                    { "device", sessionReport["device"] },
                    { "notifier", sessionReport["notifier"] },
                    { "session", ((Dictionary<string, object>[])sessionReport["sessions"])[0] }
                };
                SimpleJson.SerializeObject(serialisableSessionReport, writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                var path = SessionsDirectory + "/" + sessionReport.Id + ".session";
                WriteToDisk(jsonString,path);
                CheckForMaxCachedSessions();
            }
        }

        internal static void CacheEvent(string reportId)
        {
            var eventReport = GetPendingEventReport(reportId);
            if (eventReport == null)
            {
                return;
            }
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(eventReport, writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                var path = EventsDirectory + "/" + eventReport.Id + ".event";
                WriteToDisk(jsonString, path);
                CheckForMaxCachedEvents();
            }
        }

        private static void CheckForMaxCachedEvents()
        {
            var filesCount = _cachedEvents.Length;
            while (filesCount > _configuration.MaxPersistedEvents)
            {
                RemoveOldestEvent();
                filesCount = _cachedEvents.Length;
            }
        }

        private static void CheckForMaxCachedSessions()
        {
            var filesCount = _cachedSessions.Length;
            while(filesCount > _configuration.MaxPersistedSessions)
            {
                RemoveOldestSession();
                filesCount = _cachedSessions.Length;
            }
        }


        private static void RemoveOldestEvent()
        {
            var oldestEvent = string.Empty;
            var oldestMillis = default(double);
            foreach (var cachedEventPath in _cachedEvents)
            {
                var milliesSinceCreated = (DateTime.UtcNow - File.GetCreationTimeUtc(cachedEventPath)).TotalMilliseconds;
                if (milliesSinceCreated > oldestMillis)
                {
                    oldestMillis = milliesSinceCreated;
                    oldestEvent = cachedEventPath;
                }
            }
            File.Delete(oldestEvent);
        }

        private static void RemoveOldestSession()
        {
            var oldestSession = string.Empty;
            var oldestMillis = default(double);
            foreach (var cachedSessionPath in _cachedSessions)
            {
                var milliesSinceCreated = (DateTime.UtcNow - File.GetCreationTimeUtc(cachedSessionPath)).TotalMilliseconds;
                if (milliesSinceCreated > oldestMillis)
                {
                    oldestMillis = milliesSinceCreated;
                    oldestSession = cachedSessionPath;
                }
            }
            File.Delete(oldestSession);
        }

        internal static void PayloadSendSuccess(IPayload payload)
        {
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    RemovePendingSession(payload.Id);
                    RemovedCachedSession(payload.Id);
                    break;
                case PayloadType.Event:
                    RemovedCachedEvent(payload.Id);
                    RemovePendingEvent(payload.Id);
                    break;
            }
        }

        internal static void RemovedCachedEvent(string id)
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

        internal static void RemovedCachedSession(string id)
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
            foreach (var cachedSessionPath in _cachedSessions)
            {
                var json = File.ReadAllText(cachedSessionPath);
                var deserialisedSessionReport = ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary();               
                var sessionReportFromCachedPayload = new SessionReport(_configuration,deserialisedSessionReport);
                cachedPayloads.Add(sessionReportFromCachedPayload);
            }
            return cachedPayloads;
        }

        private static void WriteToDisk(string json, string path)
        {
            CheckForDirectoryCreation();
            File.WriteAllText(path, json);
        }

        private static void CheckForDirectoryCreation()
        {
            try
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    Directory.CreateDirectory(CacheDirectory);
                }
                if (!Directory.Exists(SessionsDirectory))
                {
                    Directory.CreateDirectory(SessionsDirectory);
                }
                if (!Directory.Exists(EventsDirectory))
                {
                    Directory.CreateDirectory(EventsDirectory);
                }
            }
            catch
            {
                //not possible in unit tests
            }
           
        }

    }
}
