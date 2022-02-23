using System;
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

        private static string CacheDirectory
        {
            get { return Application.persistentDataPath + "/Bugsnag"; }
        }

        private static Configuration _configuration;


        private static string SessionsDirectory
        {
            get { return CacheDirectory + "/Sessions"; }
        }

        private static string[] _cachedSessions => Directory.GetFiles(SessionsDirectory, "*.session");

        internal static void InitFileManager(Configuration configuration)
        {
            Debug.Log("Initialising the Cache Manager");
            _configuration = configuration;
            CheckForDirectoryCreation();
            Debug.Log("there are currently " + _cachedSessions.Length + " sessions saved in the cache");

        }

        internal static void AddPendingSession(SessionReport sessionReport)
        {
            Debug.Log("Added a session to the list of pending session payloads with the id: " + sessionReport.Id);
            _pendingSessions.Add(sessionReport);
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

        internal static void SendPayloadFailed(IPayload payload)
        {
            Debug.Log("sending a session with the id" + payload.Id + " failed, writing the session to disk now");

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

        private static void CheckForMaxCachedSessions()
        {
            var filesCount = _cachedSessions.Length;
            while(filesCount > _configuration.MaxPersistedSessions)
            {
                RemoveOldestSession();
                filesCount = _cachedSessions.Length;
            }
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
                    break;
            }
        }

        internal static void RemovedCachedSession(string id)
        {

            Debug.Log("Removing session file from the cache, session id: " + id);
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
            Debug.Log("Removing session from the list of pending session requests, session id: " + id);
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
            Debug.Log("Writing a session to disk with the content: " + json);
            CheckForDirectoryCreation();
            File.WriteAllText(path, json);
            Debug.Log("session was written to disk at the location: " + path);

        }

        private static void CheckForDirectoryCreation()
        {

            Debug.Log("Checking if the Bugsnag session cache already exists");


            try
            {
                if (!Directory.Exists(CacheDirectory))
                {
                    Debug.Log("The Root Cache Directory does not exist, creating it here: " + CacheDirectory);
                    Directory.CreateDirectory(CacheDirectory);
                }
                else
                {
                    Debug.Log("The Root Cache Directory already exists here: " + CacheDirectory);
                }
                if (!Directory.Exists(SessionsDirectory))
                {
                    Debug.Log("The session Cache Directory does not exist, creating it here: " + SessionsDirectory);
                    Directory.CreateDirectory(SessionsDirectory);
                }
                else
                {
                    Debug.Log("The session Cache Directory already exists here: " + SessionsDirectory);
                }
            }
            catch
            {
                //not possible in unit tests
            }
           
        }

    }
}
