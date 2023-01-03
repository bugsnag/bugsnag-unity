using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BugsnagUnity.Payload;
using UnityEngine;
namespace BugsnagUnity
{
    public class PayloadManager
    {

        private static List<PendingPayload> _pendingPayloads = new List<PendingPayload>();

        private CacheManager _cacheManager;

        private class PendingPayload
        {
            public string Json;
            public string PayloadId;

            public PendingPayload(string json, string payloadId)
            {
                Json = json;
                PayloadId = payloadId;
            }
        }

        internal PayloadManager(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        internal void AddPendingPayload(IPayload payload)
        {
            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = false })
            {
                SimpleJson.SerializeObject(payload.GetSerialisablePayload(), writer);
                writer.Flush();
                stream.Position = 0;
                var jsonString = reader.ReadToEnd();
                _pendingPayloads.Add(new PendingPayload(jsonString,payload.Id));
            }
        }

        private PendingPayload GetPendingPayload(string id)
        {
            foreach (var payload in _pendingPayloads)
            {
                if (payload.PayloadId == id)
                {
                    return payload;
                }
            }
            return null;
        }

        internal void SendPayloadFailed(IPayload payload)
        {
            switch (payload.PayloadType)
            {
                case PayloadType.Session:
                    CacheSession(payload.Id);
                    break;
                case PayloadType.Event:
                    CacheEvent(payload.Id);
                    break;
            }
        }

        internal void CacheSession(string reportId)
        {
            var pendingSession = GetPendingPayload(reportId);
            if (pendingSession == null)
            {
                return;
            }
            _cacheManager.SaveSessionToCache(pendingSession.PayloadId, pendingSession.Json);
            RemovePendingPayload(reportId);
        }

        internal void CacheEvent(string reportId)
        {
            var pendingEvent = GetPendingPayload(reportId);
            if (pendingEvent == null)
            {
                return;
            }
            _cacheManager.SaveEventToCache(pendingEvent.PayloadId, pendingEvent.Json);
            RemovePendingPayload(reportId);
        }

        internal void RemovePayload(IPayload payload)
        {
            RemovePendingPayload(payload.Id);
            if (payload.PayloadType == PayloadType.Session)
            {
                RemoveCachedSession(payload.Id);
            }
            else
            {
                RemoveCachedEvent(payload.Id);
            }
        }

        private void RemovePendingPayload(string id)
        {
            _pendingPayloads.RemoveAll(item => item.PayloadId == id);
        }

        internal void RemoveCachedEvent(string id)
        {
            _cacheManager.RemoveCachedEvent(id);
        }

        internal void RemoveCachedSession(string id)
        {
            _cacheManager.RemoveCachedSession(id);
        }
      
    }
}
