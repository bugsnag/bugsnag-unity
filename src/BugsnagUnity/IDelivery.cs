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
    }

    class Delivery : IDelivery
    {

        const int DeliveryFailureDelay = 10000;
        Boolean DelayBeforeDelivery { get; set; } = false;
        Thread Worker { get; }

        BlockingQueue<IPayload> Queue { get; }

        GameObject DispatcherObject { get; }

        internal Delivery()
        {
            DispatcherObject = new GameObject("Bugsnag thread dispatcher");
            DispatcherObject.AddComponent<MainThreadDispatchBehaviour>();

            Queue = new BlockingQueue<IPayload>();
            if (CanUseThreading())
            {
                Worker = new Thread(ProcessQueue) { IsBackground = true };
                Worker.Start();
            }
        }

        void ProcessQueue()
        {
            while (true)
            {
                try
                {
                    if (DelayBeforeDelivery)
                    {
                        DelayBeforeDelivery = false;
                        Thread.Sleep(DeliveryFailureDelay);
                    }
                    else
                    {
                        SerializeAndDeliverPayload(Queue.Dequeue());
                    }
                }
                catch (System.Exception)
                {
                    // ensure that the thread carries on processing error reports
                }
            }
        }

        void SerializeAndDeliverPayload(IPayload payload)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                BugsnagUnity.SimpleJson.SerializeObject(payload, writer);
                writer.Flush();
                stream.Position = 0;
                var body = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, body));
            }
        }

        public void Send(IPayload payload)
        {
            if (CanUseThreading())
            {
                Queue.Enqueue(payload);
            }
            else
            {
                if (DelayBeforeDelivery)
                {
                    DelayBeforeDelivery = false;
                    MainThreadDispatchBehaviour.Instance().EnqueueWithDelayCoroutine(()=> { SerializeAndDeliverPayload(payload); }, DeliveryFailureDelay / 1000);
                }
                else
                {
                    SerializeAndDeliverPayload(payload);
                }
            }
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
                if (req.responseCode >= 200 && req.responseCode < 300)
                {
                    // success!
                    FileManager.PayloadSent(payload);
                }
                else if (req.responseCode >= 500)
                {
                    // Something is wrong with the server/connection, retry after a delay
                    DelayBeforeDelivery = true;
                    Send(payload);
                }
            }
        }

        private void TrySendingCachedPayloads()
        {
            var payloads = FileManager.GetCachedPayloads();
            foreach (var payload in payloads)
            {
                Send(payload);
            }
        }

        private bool CanUseThreading()
        {
            return Application.platform != RuntimePlatform.WebGLPlayer;
        }
    }
}
