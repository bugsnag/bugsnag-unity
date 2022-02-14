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
            get { return Application.temporaryCachePath + "/Bugsnag"; }
        }

        private static Configuration _configuration;


        private static string SessionsDirectory
        {
            get { return CacheDirectory + "/Sessions"; }
        }

        public static void InitFileManager(Configuration configuration)
        {
            _configuration = configuration;
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
                    { "session", ((PayloadDictionary[])sessionReport["sessions"])[0] }
                };
                SimpleJson.SerializeObject(serialisableSessionReport, writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                var path = SessionsDirectory + "/" + sessionReport.Id + ".session";
                WriteToDisk(jsonString,path);
                Debug.Log("Session written to " + path);
            }
            
        }

        internal static void PayloadSent(IPayload payload)
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
            return;
            Debug.Log("Removing cached session " + id);
            foreach (var cachedSessionPath in Directory.GetFiles(SessionsDirectory))
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
            foreach (var cachedSessionPath in Directory.GetFiles(SessionsDirectory))
            {
                if (!cachedSessionPath.Contains(".session"))
                {
                    continue;
                }
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
