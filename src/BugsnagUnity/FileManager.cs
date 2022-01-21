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


        private static string SessionsDirectory
        {
            get { return CacheDirectory + "/Sessions"; }
        }

        internal static void CacheSession(SessionReport sessionReport)
        {
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
                    WriteToDisk(jsonString,SessionsDirectory + "/" + sessionReport.Id + ".json");
                }
            }
        }

        internal static void PayloadSent(IPayload payload)
        {
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
                var json = File.ReadAllText(cachedSessionPath);
                var sessionReport = SimpleJson.DeserializeObject<SessionReport>(json);
                cachedPayloads.Add(sessionReport);
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
