using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnity
{
    class Delivery
    {

        private GameObject _dispatcherObject;
        private Configuration _configuration;
        private object _callbackLock { get; } = new object();

        private static List<string> _finishedCacheDeliveries = new List<string>();

        private bool _cacheDeliveryInProcess;
        private WaitForSeconds _deliverCacheWaitTime = new WaitForSeconds(0.1f);

        internal Delivery(Configuration configuration)
        {
            _configuration = configuration;
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
                catch { }
            }
        }

        public void Send(IPayload payload)
        {
            if (payload.PayloadType == PayloadType.Event)
            {
                var report = (Report)payload;

                lock (_callbackLock)
                {
                    foreach (var onSendErrorCallback in _configuration.GetOnSendErrorCallbacks())
                    {
                        try
                        {
                            if (!onSendErrorCallback.Invoke(report.Event))
                            {
                                return;
                            }
                        }
                        catch
                        {
                            // If the callback causes an exception, ignore it and execute the next one
                        }
                    }
                }
                report.Event.RedactMetadata(_configuration);
                report.ApplyEventPayload();
            }
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
                _finishedCacheDeliveries.Add(payload.Id);
            }
        }

        public void TrySendingCachedPayloads()
        {
            if (_cacheDeliveryInProcess)
            {
                return;
            }
            _cacheDeliveryInProcess = true;
            try
            {
                _finishedCacheDeliveries.Clear();
                var payloads = FileManager.GetCachedPayloadPaths();
                MainThreadDispatchBehaviour.Instance().Enqueue(DeliverCachedPayloads(payloads));
            }
            catch
            {
                // Not possible in unit tests
            }
        }

        private bool CachedPayloadProcessed(string id)
        {
            foreach (var processedCachedPayloadIds in _finishedCacheDeliveries)
            {
                if (id == processedCachedPayloadIds)
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerator DeliverCachedPayloads(string[] payloadPaths)
        {
            foreach (var cachedPayloadPath in payloadPaths)
            {
                var payload = FileManager.GetPayloadFromCachePath(cachedPayloadPath);
                if (payload != null)
                {
                    Send(payload);
                    while (!CachedPayloadProcessed(payload.Id))
                    {
                        yield return _deliverCacheWaitTime;
                    }
                }
            }
            _cacheDeliveryInProcess = false;
        }

    }
}
