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
            if (sessionReport != null)
            {
                using (var stream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
                {
                    SimpleJson.SerializeObject(sessionReport, writer);
                    writer.Flush();
                    stream.Position = 0;
                    var jsonString = reader.ReadToEnd();
                    var path = SessionsDirectory + "/" + sessionReport.Id + ".session";
                    WriteToDisk(jsonString,path);
                    Debug.Log("Session written to " + path);
                }
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
            var cachedPayloads = new List<IPayload>();
            foreach (var cachedSessionPath in Directory.GetFiles(SessionsDirectory))
            {
                if (!cachedSessionPath.Contains(".session"))
                {
                    continue;
                }
                Debug.Log("The path: " + cachedSessionPath);
                var json = File.ReadAllText(cachedSessionPath);
                Debug.Log("The json: " + json);
                var sessionReport = SimpleJson.DeserializeObject<Dictionary<string,object>>(json);
                Debug.Log("got cached sessionReport: " + sessionReport.Count);
               
                Debug.Log("sessionReport session type: " + sessionReport.Get("sessions").GetType().Name);



                var sessionFromPayload = new SessionReport(_configuration,sessionReport);
                Debug.Log("got cached session payload: " + sessionFromPayload.Id);

                // cachedPayloads.Add(sessionReport);
            }
            throw new Exception("boop");
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
