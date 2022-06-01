using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface ICacheManager
    {
        string GetCachedDeviceId();
        void SaveDeviceIdToCache(string id);
        void CacheSession(string id, string json);
        void CacheEvent(string id, string json);
        void RemoveCachedEvent(string id);
        void RemoveCachedSession(string id);
        IPayload GetNextCachedPayload();
    }
}
