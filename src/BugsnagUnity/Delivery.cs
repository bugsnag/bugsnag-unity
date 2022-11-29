﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using BugsnagUnity.Payload;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsnagUnity
{
    class Delivery
    {

        private Client _client;

        private Configuration _configuration;

        private CacheManager _cacheManager;

        private PayloadManager _payloadManager;

        private object _callbackLock { get; } = new object();

        private static List<string> _finishedCacheDeliveries = new List<string>();

        private bool _cacheDeliveryInProcess;

        private const int MAX_PAYLOAD_BYTES = 1000000;

        private const string STRING_TRUNCATION_MESSAGE = "***{0} CHARS TRUNCATED***";

        private const string BREADCRUMB_TRUNCATION_MESSAGE = "Removed, along with {0} older breadcrumbs, to reduce payload size";

        private const string EVENT_KEY_EVENT = "event";

        private const string EVENT_KEY_BREADCRUMBS = "breadcrumbs";

        private const string EVENT_KEY_METADATA = "metaData";

        private const string EVENT_KEY_BREADCRUMB_TYPE = "type";

        private const string EVENT_KEY_BREADCRUMB_MESSAGE = "name";

        private const string EVENT_KEY_BREADCRUMB_METADATA = "metaData";


        internal Delivery(Client client, Configuration configuration, CacheManager cacheManager, PayloadManager payloadManager)
        {
            _client = client;
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
                MainThreadDispatchBehaviour.Instance().Enqueue(PushToServer(payload));
            }
            catch
            {
                // not avaliable in unit tests
            }
        }

        private byte[] TruncateBreadcrumbs(IPayload payload, byte[] serialisedPayload)
        {
            var @event = payload.GetSerialisablePayload()[EVENT_KEY_EVENT] as PayloadDictionary;
            var breadcrumbsList = (@event[EVENT_KEY_BREADCRUMBS] as Dictionary<string, object>[]).ToList();

            if (breadcrumbsList.Count == 0)
            {
                return serialisedPayload;
            }

            var bytesToRemove = serialisedPayload.Length - MAX_PAYLOAD_BYTES;
            var numBreadcrumbsRemoved = 0;
            var numBytesTruncated = 0;
            var lastRemovedCrumbType = string.Empty;

            for (int i = breadcrumbsList.Count - 1; i >= 0; i--)
            {
                numBytesTruncated += GetBreadcrumbSize(breadcrumbsList[i]);
                lastRemovedCrumbType = breadcrumbsList[i][EVENT_KEY_BREADCRUMB_TYPE] as string;
                breadcrumbsList.RemoveAt(i);
                numBreadcrumbsRemoved++;
                if (numBytesTruncated >= bytesToRemove)
                {
                    break;
                }
            }

            if (numBreadcrumbsRemoved > 0)
            {
                var truncationBreadcrumb = new Dictionary<string, object>
                {
                    { EVENT_KEY_BREADCRUMB_TYPE, lastRemovedCrumbType },
                    { EVENT_KEY_BREADCRUMB_MESSAGE, string.Format(BREADCRUMB_TRUNCATION_MESSAGE, numBreadcrumbsRemoved) }
                };
                breadcrumbsList.Add(truncationBreadcrumb);
                @event[EVENT_KEY_BREADCRUMBS] = breadcrumbsList.ToArray();
                return SerializePayload(payload);
            }
            else
            {
                return serialisedPayload;
            }
        }

        private int GetBreadcrumbSize(Dictionary<string, object> data)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(data, writer);
                writer.Flush();
                stream.Position = 0;
                return Encoding.UTF8.GetBytes(reader.ReadToEnd()).Length;
            }
        }

        private byte[] TruncateMetadata(IPayload payload)
        {
            var @event = payload.GetSerialisablePayload()[EVENT_KEY_EVENT] as PayloadDictionary;
            var metadata = @event[EVENT_KEY_METADATA] as PayloadDictionary;
            foreach (var section in metadata)
            {
                TruncateStringsInDictionary(section.Value as Dictionary<string,object>);
            }

            var breadcrumbs = @event[EVENT_KEY_BREADCRUMBS] as Dictionary<string, object>[];
            foreach (var crumb in breadcrumbs)
            {
                var crumbMetadata = crumb[EVENT_KEY_BREADCRUMB_METADATA] as Dictionary<string, object>;
                TruncateStringsInDictionary(crumbMetadata);
            }

            return SerializePayload(payload);
        }

        private void TruncateStringsInDictionary(Dictionary<string, object> section)
        {
            foreach (var key in section.Keys.ToList())
            {
                var valueType = section[key].GetType();
                if (valueType == typeof(string))
                {
                    var originalValue = section[key] as string;
                    section[key] = TruncateStringIfNecessary(originalValue);
                }
                else if (valueType == typeof(string[]))
                {
                    var stringArray = section[key] as string[];
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        stringArray[i] = TruncateStringIfNecessary(stringArray[i]);
                    }
                }
                else if (valueType == typeof(List<string>))
                {
                    var stringArray = section[key] as List<string>;
                    for (int i = 0; i < stringArray.Count; i++)
                    {
                        stringArray[i] = TruncateStringIfNecessary(stringArray[i]);
                    }
                }
                else if (valueType == typeof(Dictionary<string, string>))
                {
                    var stringDict = section[key] as Dictionary<string, string>;
                    foreach (var stringKey in stringDict.Keys.ToList())
                    {
                        stringDict[stringKey] = TruncateStringIfNecessary(stringDict[stringKey]);
                    }
                }
                else if (valueType == typeof(Dictionary<string, object>))
                {
                    TruncateStringsInDictionary(section[key] as Dictionary<string, object>);
                }
                else if (valueType == typeof(JsonArray))
                {
                    var array = ((JsonArray)section[key]);
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i].GetType() == typeof(string))
                        {
                            array[i] = TruncateStringIfNecessary((string)array[i]);
                        }
                    }
                }
            }
        }

        private string TruncateStringIfNecessary(string originalValue)
        {
            if (originalValue.Length > _configuration.MaxStringValueLength)
            {
                var numStringsToTruncate = originalValue.Length - _configuration.MaxStringValueLength;
                var truncationMessage = GetTruncationMessage(numStringsToTruncate);
                if (numStringsToTruncate >= truncationMessage.Length)
                {
                    return TruncateString(originalValue, truncationMessage);
                }
            }
            return originalValue;
        }

        private string TruncateString(string stringToTruncate, string truncationMessage)
        {
            return stringToTruncate.Substring(0, _configuration.MaxStringValueLength) + truncationMessage;
        }

        private string GetTruncationMessage(int numCharactersToRemove)
        {
            return string.Format(STRING_TRUNCATION_MESSAGE, numCharactersToRemove);
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

        private byte[] PreaparePayloadBody(IPayload payload)
        {
            var serialisedPayload = SerializePayload(payload);
            //// If payload is oversized, trim string values in all metadata
            if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
            {
                serialisedPayload = TruncateMetadata(payload);
            }
            // If still oversized, truncate the breadcrumbs
            if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
            {
                serialisedPayload = TruncateBreadcrumbs(payload, serialisedPayload);
            }
            return serialisedPayload;
        }
        

        // Push to the server and handle the result
        IEnumerator PushToServer(IPayload payload)
        {
            var shouldDeliver = false;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                shouldDeliver = _client.NativeClient.ShouldAttemptDelivery();
            }
            else
            {
                var networkCheckDone = false;
                new Thread(() => {
                    shouldDeliver = _client.NativeClient.ShouldAttemptDelivery();
                    networkCheckDone = true;
                }).Start();

                while (!networkCheckDone)
                {
                    yield return null;
                }
            }
            if (!shouldDeliver)
            {
                _payloadManager.SendPayloadFailed(payload);
                _finishedCacheDeliveries.Add(payload.Id);
                yield break;
            }

            byte[] body = new byte[] { };
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                body = PreaparePayloadBody(payload);
            }
            else
            {
                var bodyReady = false;
                new Thread(() => {
                    body = PreaparePayloadBody(payload);
                    bodyReady = true;
                }).Start();

                while (!bodyReady)
                {
                    yield return null;
                }
            }

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
                    StartDeliveringCachedPayloads();
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
            var cachedSessionIds = _cacheManager.GetCachedSessionIds();
            if (cachedSessionIds != null)
            {
                foreach (var sessionId in cachedSessionIds)
                {
                    var sessionJson = _cacheManager.GetCachedSession(sessionId);
                    if (string.IsNullOrEmpty(sessionJson))
                    {
                        continue;
                    }
                    var sessionReport = new SessionReport(_configuration, ((JsonObject)SimpleJson.DeserializeObject(sessionJson)).GetDictionary());
                    Deliver(sessionReport);
                    yield return new WaitUntil(() => CachedPayloadProcessed(sessionReport.Id));
                }
            }

            var cachedEvents = _cacheManager.GetCachedEventIds();
            if (cachedEvents != null)
            {
                foreach (var eventId in cachedEvents)
                {
                    var eventJson = _cacheManager.GetCachedEvent(eventId);
                    if (string.IsNullOrEmpty(eventJson))
                    {
                        continue;
                    }
                    var eventReport = new Report(_configuration, ((JsonObject)SimpleJson.DeserializeObject(eventJson)).GetDictionary());
                    Deliver(eventReport);
                    yield return new WaitUntil(() => CachedPayloadProcessed(eventReport.Id));
                }
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
