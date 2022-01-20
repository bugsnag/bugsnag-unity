using System;
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


        internal static void CacheSession(Session session)
        {
            if (session != null)
            {
                using (var stream = new MemoryStream())
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
                {
                    SimpleJson.SerializeObject(session.Payload, writer);
                    writer.Flush();
                    stream.Position = 0;
                    var jsonString = reader.ReadToEnd();
                    
                }
            }
        }

    }
}
var tw = new StreamWriter(GetSaveFilePath());
if (_config.ScrambleSaveData)
{
    data = DataScrambler(data);
}
tw.Write(data);
tw.Close();