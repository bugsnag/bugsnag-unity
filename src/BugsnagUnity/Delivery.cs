using System;
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

        private const string EVENT_KEY_BREADCRUMB_TIMESTAMP = "timestamp";


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
                //pipeline expects and array of events even though we only ever report 1
                report.ApplyEventsArray();
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

            byte[] body = null;
            // It's not possible to yield from a try catch block, so we use this flag bool instead
            var errorDuringSerialisation = false;
            if (payload.PayloadType == PayloadType.Session)
            {
                try
                {
                    body = SerializeObject(payload);
                }
                catch
                {
                    errorDuringSerialisation = true;
                }
            }
            else
            {
                // There is no threading on webgl, so we treat the payload differently
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    try
                    {
                        body = PrepareEventBodySimple(payload);
                    }
                    catch
                    {
                        errorDuringSerialisation = true;
                    }
                }
                else
                {
                    var bodyReady = false;
                    new Thread(() => {
                        try
                        {
                            body = PrepareEventBody(payload);
                        }
                        catch
                        {
                            errorDuringSerialisation = true;
                        }
                        bodyReady = true;
                    }).Start();

                    while (!bodyReady)
                    {
                        yield return null;
                    }
                }
            }

            if (errorDuringSerialisation)
            {
                _payloadManager.RemovePayload(payload);
                _finishedCacheDeliveries.Add(payload.Id);
                yield break;
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
                    _payloadManager.RemovePayload(payload);
                    StartDeliveringCachedPayloads();
                }
                else if ( code == 0 || code == 408 || code == 429 || code >= 500)
                {
                    // sending failed with no network or retryable error, cache payload to disk
                    _payloadManager.SendPayloadFailed(payload);
                }
                else
                {
                    // sending failed with an unacceptable status code, remove payload from cache and pending payloads
                    _payloadManager.RemovePayload(payload);
                }
                _finishedCacheDeliveries.Add(payload.Id);
            }
        }

        private Dictionary<string, object> GetEventFromPayload(Dictionary<string, object> payloadAsDictionary)
        {
            return payloadAsDictionary[EVENT_KEY_EVENT] as Dictionary<string, object>;
        }

        private byte[] PrepareEventBody(IPayload payload)
        {
            var serialisedPayload = SerializeObject(payload);
            // If payload is oversized, trim string values in all metadata
            if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
            {
                var @event = GetEventFromPayload(payload.GetSerialisablePayload());
                if (TruncateMetadata(@event))
                {
                    serialisedPayload = SerializeObject(payload);
                }

                //If still oversized, truncate the breadcrumbs
                if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
                {
                    if (TruncateBreadcrumbs(@event, serialisedPayload.Length - MAX_PAYLOAD_BYTES))
                    {
                        serialisedPayload = SerializeObject(payload);
                    }
                }
            }
           
            return serialisedPayload;
        }

        private byte[] PrepareEventBodySimple(IPayload payload)
        {
            var serialisedPayload = SerializeObject(payload);

            // If payload is oversized, remove all breadcrumbs
            if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
            {
                var @event = payload.GetSerialisablePayload();

                if (RemoveAllBreadcrumbs(@event))
                {
                    serialisedPayload = SerializeObject(payload);
                }

                //If still oversized, clear out all metadata
                if (serialisedPayload.Length > MAX_PAYLOAD_BYTES)
                {
                    if (RemoveUserMetadata(@event))
                    {
                        serialisedPayload = SerializeObject(payload);
                    }
                }
            }
           
            return serialisedPayload;
        }

        private bool TruncateBreadcrumbs(Dictionary<string, object> @event, int bytesToRemove)
        {
            var breadcrumbsList = (@event[EVENT_KEY_BREADCRUMBS] as Dictionary<string, object>[]).ToList();

            if (breadcrumbsList.Count == 0)
            {
                return false;
            }

            var numBreadcrumbsRemoved = 0;
            var numBytesTruncated = 0;
            var lastRemovedCrumbType = string.Empty;

            for (int i = breadcrumbsList.Count - 1; i >= 0; i--)
            {
                numBytesTruncated += SerializeObject(breadcrumbsList[i]).Length;
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
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool RemoveAllBreadcrumbs(Dictionary<string, object> @event)
        {
            var numExistingCrumbs = (@event[EVENT_KEY_BREADCRUMBS] as Dictionary<string, object>[]).Length;

            if (numExistingCrumbs == 0)
            {
                return false;
            }

            var truncationBreadcrumb = new Dictionary<string, object>
            {
                { EVENT_KEY_BREADCRUMB_TIMESTAMP, DateTime.UtcNow.ToString() },
                { EVENT_KEY_BREADCRUMB_TYPE, "state" },
                { EVENT_KEY_BREADCRUMB_MESSAGE, string.Format(BREADCRUMB_TRUNCATION_MESSAGE, numExistingCrumbs) }
            };

            @event[EVENT_KEY_BREADCRUMBS] = new[] { truncationBreadcrumb };

            return true;
        }

        private bool TruncateMetadata(Dictionary<string, object> @event)
        {
            var dataTruncated = false;
            var metadata = @event[EVENT_KEY_METADATA] as Dictionary<string, object>;
            foreach (var section in metadata)
            {
                if (TruncateStringsInDictionary(section.Value as Dictionary<string, object>))
                {
                    dataTruncated = true;
                }
            }

            var breadcrumbs = @event[EVENT_KEY_BREADCRUMBS] as Dictionary<string, object>[];
            foreach (var crumb in breadcrumbs)
            {
                var crumbMetadata = crumb[EVENT_KEY_BREADCRUMB_METADATA] as Dictionary<string, object>;
                if (TruncateStringsInDictionary(crumbMetadata))
                {
                    dataTruncated = true;
                }
            }
            return dataTruncated;
        }

        private bool RemoveUserMetadata(Dictionary<string, object> @event)
        {
            var dataRemoved = false;
            var metadata = @event[EVENT_KEY_METADATA] as Dictionary<string, object>;
            foreach (var key in metadata.Keys.ToList())
            {
                if (key != "app" && key != "device")
                {
                    dataRemoved = true;
                    metadata.Remove(key);
                }
            }
            return dataRemoved;
        }

        private bool TruncateStringsInDictionary(Dictionary<string, object> section)
        {
            var stringTruncated = false;
            foreach (var key in section.Keys.ToList())
            {
                var valueType = section[key].GetType();
                if (valueType == typeof(string))
                {
                    var originalValue = section[key] as string;
                    if (ShouldTruncateString(originalValue))
                    {
                        section[key] = TruncateString(originalValue);
                        stringTruncated = true;
                    }
                }
                else if (valueType == typeof(string[]))
                {
                    var stringArray = section[key] as string[];
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        if (ShouldTruncateString(stringArray[i]))
                        {
                            stringArray[i] = TruncateString(stringArray[i]);
                            stringTruncated = true;
                        }
                    }
                }
                else if (valueType == typeof(List<string>))
                {
                    var stringList = section[key] as List<string>;
                    for (int i = 0; i < stringList.Count; i++)
                    {
                        if (ShouldTruncateString(stringList[i]))
                        {
                            stringList[i] = TruncateString(stringList[i]);
                            stringTruncated = true;
                        }
                    }
                }
                else if (valueType == typeof(Dictionary<string, string>))
                {
                    var stringDict = section[key] as Dictionary<string, string>;
                    foreach (var stringKey in stringDict.Keys.ToList())
                    {
                        if (ShouldTruncateString(stringDict[stringKey]))
                        {
                            stringTruncated = true;
                            stringDict[stringKey] = TruncateString(stringDict[stringKey]);
                        }
                    }
                }
                else if (valueType == typeof(Dictionary<string, object>))
                {
                    TruncateStringsInDictionary(section[key] as Dictionary<string, object>);
                }
                else if (valueType == typeof(JsonArray))
                {
                    var jsonArray = ((JsonArray)section[key]);
                    for (int i = 0; i < jsonArray.Count; i++)
                    {
                        if (jsonArray[i].GetType() == typeof(string))
                        {
                            var originalValue = jsonArray[i] as string;
                            if (ShouldTruncateString(originalValue))
                            {
                                jsonArray[i] = TruncateString(originalValue);
                                stringTruncated = true;
                            }
                        }
                    }
                }
            }
            return stringTruncated;
        }

        private bool ShouldTruncateString(string stringValue)
        {
            return stringValue.Length > _configuration.MaxStringValueLength;
        }

        private string TruncateString(string originalValue)
        {
            var numStringsToTruncate = originalValue.Length - _configuration.MaxStringValueLength;
            var truncationMessage = GetTruncationMessage(numStringsToTruncate);
            if (numStringsToTruncate >= truncationMessage.Length)
            {
                return TruncateString(originalValue, truncationMessage);
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
            yield return ProcessCachedItems(typeof(SessionReport));
            yield return ProcessCachedItems(typeof(Report));
            _cacheDeliveryInProcess = false;
        }

        private IEnumerator ProcessCachedItems(Type t)
        {
            bool isSession = t.Equals(typeof(SessionReport));
            var cachedPayloads = isSession ? _cacheManager.GetCachedSessionIds() : _cacheManager.GetCachedEventIds();
            if (cachedPayloads != null)
            {
                foreach (var id in cachedPayloads)
                {
                    var payloadJson = isSession ? _cacheManager.GetCachedSession(id) : _cacheManager.GetCachedEvent(id);
                    if (string.IsNullOrEmpty(payloadJson))
                    {
                        continue;
                    }

                    Dictionary<string, object> payloadDictionary = null;

                    try
                    {
                        // if something goes wrong at this stage then we silently discard the file as it's most likely that the file wasn't fully serialised to disk
                        payloadDictionary = ((JsonObject)SimpleJson.DeserializeObject(payloadJson)).GetDictionary();
                    }
                    catch
                    {
                        if (isSession)
                        {
                            _cacheManager.RemoveCachedSession(id);
                        }
                        else
                        {
                            _cacheManager.RemoveCachedEvent(id);
                        }
                        continue;
                    }


                    // if something goes wrong at this stage then we discard the file and report the error as it might be a bug in the sdk
                    if (isSession)
                    {
                        try
                        {
                            var sessionReport = new SessionReport(_configuration, payloadDictionary);
                            Deliver(sessionReport);
                        }
                        catch
                        {
                            _cacheManager.RemoveCachedSession(id);
                            continue;
                        }
                    }
                    else
                    {
                        try
                        {
                            var report = new Report(_configuration, payloadDictionary);
                            Deliver(report);
                        }
                        catch
                        {
                            _cacheManager.RemoveCachedEvent(id);
                            continue;
                        }
                    }
                    
                    yield return new WaitUntil(() => CachedPayloadProcessed(id));
                }
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

        private byte[] SerializeObject(object @object)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(@object, writer);
                writer.Flush();
                stream.Position = 0;
                return Encoding.UTF8.GetBytes(reader.ReadToEnd());
            }
        }

    }
}
