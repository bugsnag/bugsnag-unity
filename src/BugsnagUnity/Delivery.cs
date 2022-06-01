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

        private Configuration _configuration;

        private CacheManager _cacheManager;

        private PayloadManager _payloadManager;

        private object _callbackLock { get; } = new object();

        private static List<string> _finishedCacheDeliveries = new List<string>();

        private bool _cacheDeliveryInProcess;


        internal Delivery(Configuration configuration, CacheManager cacheManager, PayloadManager payloadManager)
        {
            _configuration = configuration;
            _cacheManager = cacheManager;
            _payloadManager = payloadManager;
        }

        // Run any on send error callbacks if it's an event, serialise the payload and add it to the sending queue
        public void Deliver(IPayload payload)
        {
            if (payload.PayloadType == PayloadType.Event)
            {
                var report = (Report)payload;
                if (_configuration.GetOnSendErrorCallbacks().Count > 0)
                {
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
                }
                report.Event.RedactMetadata(_configuration);
                report.ApplyEventPayload();
            }
            try
            {
                // not avaliable in unit tests
                MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload, SerializePayload(payload)));
            }
            catch { }
        }

        byte[] SerializePayload(IPayload payload)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(payload, writer);
                writer.Flush();
                stream.Position = 0;
                return Encoding.UTF8.GetBytes(reader.ReadToEnd());
            }
        }

        // Push to the server and handle the result
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
                    _payloadManager.PayloadSendSuccess(payload);
                }
                else if (req.isNetworkError || code == 0 || code == 408 || code == 429 || code >= 500)
                {
                    // sending failed with no network or retryable error, cache payload to disk
                    _payloadManager.SendPayloadFailed(payload);
                }
                _finishedCacheDeliveries.Add(payload.Id);
            }
        }

        public void StartDeliveringCachedPayloads()
        {
            if (_cacheDeliveryInProcess)
            {
                return;
            }
            _cacheDeliveryInProcess = true;
            try
            {
                _finishedCacheDeliveries.Clear();
                MainThreadDispatchBehaviour.Instance().Enqueue(DeliverCachedPayloads());
            }
            catch
            {
                // Not possible in unit tests
            }
        }

        private IEnumerator DeliverCachedPayloads()
        {
            var payload = _cacheManager.GetNextCachedPayload();
            while (payload != null)
            {
                Deliver(payload);
                yield return new WaitUntil(() => CachedPayloadProcessed(payload.Id));
                payload = _cacheManager.GetNextCachedPayload();
            }
            _cacheDeliveryInProcess = false;
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

    }
}
