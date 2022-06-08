using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface ICacheManager
    {
        string GetCachedDeviceId();
        void SaveDeviceIdToCache(string id);
        void SaveSessionToCache(string id, string json);
        void SaveEventToCache(string id, string json);
        void RemoveCachedEvent(string id);
        void RemoveCachedSession(string id);
        string[] GetCachedPayloadIds();
        IPayload GetCachedPayload(string id);
    }
}
