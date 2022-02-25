﻿using System;
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
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
               
                SimpleJson.SerializeObject(sessionReport.GetSerialisableSessionReport(), writer);
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
                SimpleJson.SerializeObject(eventReport.GetSerialisableEventReport(), writer);
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
                RemoveOldestFile(_cachedEvents);
                filesCount = _cachedEvents.Length;
            }
        }

        private static void CheckForMaxCachedSessions()
        {
            var filesCount = _cachedSessions.Length;
            while(filesCount > _configuration.MaxPersistedSessions)
            {
                RemoveOldestFile(_cachedSessions);
                filesCount = _cachedSessions.Length;
            }
        }

        private static void RemoveOldestFile(string[] filePaths)
        {
            var oldestFilePath = string.Empty;
            var oldestMillis = default(double);
            foreach (var filePath in filePaths)
            {
                var milliesSinceCreated = (DateTime.UtcNow - File.GetCreationTimeUtc(filePath)).TotalMilliseconds;
                if (milliesSinceCreated > oldestMillis)
                {
                    oldestMillis = milliesSinceCreated;
                    oldestFilePath = filePath;
                }
            }
            if (!string.IsNullOrEmpty(oldestFilePath))
            {
                File.Delete(oldestFilePath);
            }
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
                    RemovedCachedEvent(eventReportFromCachedPayload.Id);
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

        private static void WriteToDisk(string json, string path)
        {
            CheckForDirectoryCreation();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
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
