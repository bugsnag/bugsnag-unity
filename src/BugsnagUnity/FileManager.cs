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

        public static void InitFileManager(Configuration configuration)
        {
            _configuration = configuration;
            CheckForDirectoryCreation();
        }

        internal static void CacheSession(SessionReport sessionReport)
        {
            Debug.Log("Caching session " + sessionReport.Id);
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                // Use a dictionary that breaks out the session from the sessions array due to issues deserialising that array later
                var serialisableSessionReport = new Dictionary<string, object>
                {
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
                Debug.Log("Session written to " + path);
                CheckForMaxCachedSessions();
            }
        }

        private static void CheckForMaxCachedSessions()
        {
            Debug.Log("Checking max cached sessions");
            var filesCount = _cachedSessions.Length;
            Debug.Log("Num cached sessions: " + filesCount);
            while(filesCount > _configuration.MaxPersistedSessions)
            {
                RemoveOldestSession();
                filesCount = _cachedSessions.Length;
            }
            Debug.Log("Num cached sessions after trimming: " + filesCount);
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
            Debug.Log("Payload sent " + payload.Id);
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    RemovedCachedSession(payload.Id);
                    break;
                case PayloadType.Event:
                    break;
            }
        }

        internal static void RemovedCachedSession(string id)
        {
            Debug.Log("Removing cached session " + id);
            foreach (var cachedSessionPath in _cachedSessions)
            {
                if (cachedSessionPath.Contains(id))
                {
                    Debug.Log("Session successfully sent, removign cached session at: " + cachedSessionPath);
                    File.Delete(cachedSessionPath);
                }
            }
        }

        internal static List<IPayload> GetCachedPayloads()
        {
            Debug.Log("Collecting Cached Payloads");
            var cachedPayloads = new List<IPayload>();
            foreach (var cachedSessionPath in _cachedSessions)
            {
                Debug.Log("Found a session at: " + cachedSessionPath);
                var json = File.ReadAllText(cachedSessionPath);
                Debug.Log("The session json: " + json);
                var deserialisedSessionReport = SimpleJson.DeserializeObject<Dictionary<string,object>>(json);               
                var sessionReportFromCachedPayload = new SessionReport(_configuration,deserialisedSessionReport);
                Debug.Log("Created session payload with ID: " + sessionReportFromCachedPayload.Id);
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
            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            if (!Directory.Exists(SessionsDirectory))
            {
                Directory.CreateDirectory(SessionsDirectory);
            }
        }



    }
}
