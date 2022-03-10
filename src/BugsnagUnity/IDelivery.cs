using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnity
{
    interface IDelivery
    {
        void Send(IPayload payload);
        void TrySendingCachedPayloads();
    }

    class Delivery : IDelivery
    {

        private GameObject _dispatcherObject;

        internal Delivery()
        {
            CreateDispatchBehaviour();
        }

        private void CreateDispatchBehaviour()
        {
            _dispatcherObject = new GameObject("Bugsnag main thread dispatcher");
            _dispatcherObject.AddComponent<MainThreadDispatchBehaviour>();
        }

        void SerializeAndDeliverPayload(IPayload payload)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(payload, writer);
                writer.Flush();
                stream.Position = 0;
                var body = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                if (_dispatcherObject == null)
                {
                    CreateDispatchBehaviour();
                }
                try
                {
                    // not avaliable in unit tests
                    MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, body));
                }
                catch{}
            }
        }

        public void Send(IPayload payload)
        {
            SerializeAndDeliverPayload(payload);            
        }

        IEnumerator PushToServer(IPayload payload, byte[] body)
        {
            using (var req = new UnityWebRequest(payload.Endpoint.ToString()))
            {
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Bugsnag-Sent-At", DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture));
                foreach (var header in payload.Headers)
                {
                    req.SetRequestHeader(header.Key, header.Value);
                }
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.method = UnityWebRequest.kHttpVerbPOST;
                yield return req.SendWebRequest();

                while (!req.isDone)
                {
                    yield return new WaitForEndOfFrame();
                }
                var code = req.responseCode;
                if (code == 200 || code == 202)
                {
                    // success!
                    FileManager.PayloadSendSuccess(payload);
                }
                else if (code >= 500 || code == 408 || code == 429 || req.isNetworkError || code < 400)
                {
                    // sending failed, cache payload to disk
                    FileManager.SendPayloadFailed(payload);
                }
            }
        }

        public void TrySendingCachedPayloads()
        {
            try
            {
                var payloads = FileManager.GetCachedPayloads();
                foreach (var payload in payloads)
                {
                    Send(payload);
                }
            }
            catch
            {
                // Not possible in unit tests
            }
        }
      
    }
}
