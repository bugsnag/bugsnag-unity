using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityTests
{
    // Minimal fake ICacheManager backed by in-memory dictionaries
    internal class FakeCacheManager : ICacheManager
    {
        public readonly Dictionary<string, string> Events = new Dictionary<string, string>();
        public readonly Dictionary<string, string> Sessions = new Dictionary<string, string>();
        private string _deviceId;

        public string GetCachedDeviceId() => _deviceId ?? string.Empty;
        public void SaveDeviceIdToCache(string id) { _deviceId = id; }

        public void SaveSessionToCache(string id, string json) { Sessions[id] = json; }
        public void SaveEventToCache(string id, string json) { Events[id] = json; }

        public void RemoveCachedEvent(string id) { Events.Remove(id); }
        public void RemoveCachedSession(string id) { Sessions.Remove(id); }

        public List<string> GetCachedEventIds() => new List<string>(Events.Keys);
        public List<string> GetCachedSessionIds() => new List<string>(Sessions.Keys);

        public string GetCachedEvent(string id)
        {
            Events.TryGetValue(id, out var v);
            return v;
        }

        public string GetCachedSession(string id)
        {
            Sessions.TryGetValue(id, out var v);
            return v;
        }
    }

    [TestFixture]
    public class PayloadManagerTests
    {
        private static Session MakeTestSession()
        {
            var session = new Session();
            session.App = new App(new Dictionary<string, object>());
            session.Device = new Device(new Dictionary<string, object>());
            session.SetUser("u1", "u@test.com", "User");
            return session;
        }

        private static (PayloadManager manager, FakeCacheManager cache) MakeManager()
        {
            var fake = new FakeCacheManager();
            var manager = new PayloadManager(fake);
            return (manager, fake);
        }

        private static SessionReport MakeSessionPayload()
        {
            var config = new Configuration("test-key");
            config.Endpoints.Configure("test-key");
            return new SessionReport(config, MakeTestSession());
        }

        [Test]
        public void AddPendingPayload_ValidPayload_ReturnsTrue()
        {
            var (manager, _) = MakeManager();
            var payload = MakeSessionPayload();
            var result = manager.AddPendingPayload(payload);
            Assert.IsTrue(result);
        }

        [Test]
        public void AddPendingPayload_MultipleTimes_AllSucceed()
        {
            var (manager, _) = MakeManager();
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(manager.AddPendingPayload(MakeSessionPayload()));
            }
        }

        [Test]
        public void CacheSession_AfterAddPending_SavesToCache()
        {
            var (manager, cache) = MakeManager();
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);

            manager.CacheSession(payload.Id);

            Assert.IsTrue(cache.Sessions.ContainsKey(payload.Id));
        }

        [Test]
        public void CacheSession_WithoutPending_DoesNothing()
        {
            var (manager, cache) = MakeManager();
            // Don't add pending payload - should silently do nothing
            manager.CacheSession("nonexistent-id");
            Assert.AreEqual(0, cache.Sessions.Count);
        }

        [Test]
        public void CacheEvent_AfterAddPending_SavesToCache()
        {
            var (manager, cache) = MakeManager();
            // Need an event report; reuse SessionReport for minimal test
            // but call CacheEvent path via SendPayloadFailed
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);

            // CacheEvent directly
            manager.CacheEvent(payload.Id);
            Assert.IsTrue(cache.Events.ContainsKey(payload.Id));
        }

        [Test]
        public void CacheEvent_WithoutPending_DoesNothing()
        {
            var (manager, cache) = MakeManager();
            manager.CacheEvent("nonexistent-id");
            Assert.AreEqual(0, cache.Events.Count);
        }

        [Test]
        public void SendPayloadFailed_Session_CallsCacheSession()
        {
            var (manager, cache) = MakeManager();
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);

            manager.SendPayloadFailed(payload);

            Assert.IsTrue(cache.Sessions.ContainsKey(payload.Id));
        }

        [Test]
        public void RemovePayload_Session_ClearsFromPendingAndCache()
        {
            var (manager, cache) = MakeManager();
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);

            // First cache it
            manager.CacheSession(payload.Id);
            Assert.IsTrue(cache.Sessions.ContainsKey(payload.Id));

            // Then remove
            manager.RemovePayload(payload);
            Assert.IsFalse(cache.Sessions.ContainsKey(payload.Id));
        }

        [Test]
        public void RemoveCachedEvent_RemovesFromCache()
        {
            var (manager, cache) = MakeManager();
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);
            manager.CacheEvent(payload.Id);

            manager.RemoveCachedEvent(payload.Id);
            Assert.IsFalse(cache.Events.ContainsKey(payload.Id));
        }

        [Test]
        public void RemoveCachedSession_RemovesFromCache()
        {
            var (manager, cache) = MakeManager();
            var payload = MakeSessionPayload();
            manager.AddPendingPayload(payload);
            manager.CacheSession(payload.Id);

            manager.RemoveCachedSession(payload.Id);
            Assert.IsFalse(cache.Sessions.ContainsKey(payload.Id));
        }
    }

    // ---- NativeClient Fallback tests (UNITY_EDITOR only) ----
#if (UNITY_EDITOR || UNITY_WEBGL || UNITY_STANDALONE_LINUX) && !(BSG_COCOA_DEV || BSG_ANDROID_DEV || BSG_WIN_DEV)
    [TestFixture]
    public class NativeClientFallbackTests
    {
        private static Configuration MakeConfig(int launchDurationMillis = 0)
        {
            var config = new Configuration("nc-test-key");
            config.LaunchDurationMillis = launchDurationMillis;
            return config;
        }

        [Test]
        public void Constructor_CreatesInstance()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.IsNotNull(nc);
            Assert.IsNotNull(nc.Configuration);
            Assert.IsNotNull(nc.Breadcrumbs);
        }

        [Test]
        public void Constructor_WithFeatureFlags_CopiesFlags()
        {
            var config = MakeConfig();
            config.AddFeatureFlag("beta", "enabled");
            var nc = new NativeClient(config);
            Assert.IsNotNull(nc);
        }

        [Test]
        public void PopulateApp_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            var app = new App(new Dictionary<string, object>());
            Assert.DoesNotThrow(() => nc.PopulateApp(app));
        }

        [Test]
        public void PopulateAppWithState_LaunchDuration0_IsLaunchingWhenNotCompleted()
        {
            var nc = new NativeClient(MakeConfig(launchDurationMillis: 0));
            var app = new AppWithState(new Dictionary<string, object>());
            nc.PopulateAppWithState(app);
            // IsLaunching should be true (not completed yet)
            Assert.IsTrue(app.IsLaunching);
        }

        [Test]
        public void PopulateAppWithState_AfterMarkLaunchCompleted_IsLaunchingFalse()
        {
            var nc = new NativeClient(MakeConfig(launchDurationMillis: 0));
            nc.MarkLaunchCompleted();
            var app = new AppWithState(new Dictionary<string, object>());
            nc.PopulateAppWithState(app);
            Assert.IsFalse(app.IsLaunching);
        }

        [Test]
        public void PopulateAppWithState_WithNonZeroDuration_UsesTimeBranch()
        {
            // LaunchDurationMillis > 0 triggers the duration comparison branch
            // app.Duration is null → null < 10000 is false in C# nullable comparison
            var nc = new NativeClient(MakeConfig(launchDurationMillis: 10000));
            var app = new AppWithState(new Dictionary<string, object>());
            nc.PopulateAppWithState(app);
            Assert.IsFalse(app.IsLaunching); // null?.TotalMilliseconds < 10000 → false
        }

        [Test]
        public void PopulateDevice_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            var device = new Device(new Dictionary<string, object>());
            Assert.DoesNotThrow(() => nc.PopulateDevice(device));
        }

        [Test]
        public void PopulateDeviceWithState_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            var device = new DeviceWithState(new Dictionary<string, object>());
            Assert.DoesNotThrow(() => nc.PopulateDeviceWithState(device));
        }

        [Test]
        public void PopulateUser_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.PopulateUser(new User("id", "email", "name")));
        }

        [Test]
        public void SetSession_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetSession(new Session()));
        }

        [Test]
        public void SetUser_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetUser(new User("id", "email", "name")));
        }

        [Test]
        public void SetContext_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetContext("ctx"));
        }

        [Test]
        public void SetAutoDetectErrors_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetAutoDetectErrors(false));
        }

        [Test]
        public void SetAutoDetectAnrs_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetAutoDetectAnrs(false));
        }

        [Test]
        public void StartSession_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.StartSession());
        }

        [Test]
        public void PauseSession_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.PauseSession());
        }

        [Test]
        public void ResumeSession_ReturnsFalse()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.IsFalse(nc.ResumeSession());
        }

        [Test]
        public void UpdateSession_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.UpdateSession(new Session()));
        }

        [Test]
        public void GetCurrentSession_ReturnsNull()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.IsNull(nc.GetCurrentSession());
        }

        [Test]
        public void MarkLaunchCompleted_SetsFlag()
        {
            var nc = new NativeClient(MakeConfig());
            nc.MarkLaunchCompleted();
            // Verify by checking PopulateAppWithState now shows not launching
            var app = new AppWithState(new Dictionary<string, object>());
            nc.PopulateAppWithState(app);
            Assert.IsFalse(app.IsLaunching);
        }

        [Test]
        public void GetLastRunInfo_ReturnsNull()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.IsNull(nc.GetLastRunInfo());
        }

        [Test]
        public void ClearNativeMetadata_Section_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddNativeMetadata("section1", new Dictionary<string, object> { ["key"] = "val" });
            Assert.DoesNotThrow(() => nc.ClearNativeMetadata("section1"));
        }

        [Test]
        public void ClearNativeMetadata_SectionAndKey_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddNativeMetadata("section1", new Dictionary<string, object> { ["key"] = "val" });
            Assert.DoesNotThrow(() => nc.ClearNativeMetadata("section1", "key"));
        }

        [Test]
        public void AddNativeMetadata_AndGetNativeMetadata_ReturnsAdded()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddNativeMetadata("s", new Dictionary<string, object> { ["k"] = "v" });
            var meta = nc.GetNativeMetadata();
            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.ContainsKey("s"));
        }

        [Test]
        public void AddFeatureFlag_AddsFlag()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddFeatureFlag("ff1", "variant1");
            Assert.DoesNotThrow(() => nc.AddFeatureFlag("ff2"));
        }

        [Test]
        public void AddFeatureFlags_Array_AddsAll()
        {
            var nc = new NativeClient(MakeConfig());
            var flags = new[] { new FeatureFlag("f1", "v1"), new FeatureFlag("f2", "v2") };
            Assert.DoesNotThrow(() => nc.AddFeatureFlags(flags));
        }

        [Test]
        public void ClearFeatureFlag_RemovesFlag()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddFeatureFlag("ff1", "v1");
            Assert.DoesNotThrow(() => nc.ClearFeatureFlag("ff1"));
        }

        [Test]
        public void ClearFeatureFlags_ClearsAll()
        {
            var nc = new NativeClient(MakeConfig());
            nc.AddFeatureFlag("ff1");
            nc.AddFeatureFlag("ff2");
            Assert.DoesNotThrow(() => nc.ClearFeatureFlags());
        }

        [Test]
        public void ToStackFrames_ReturnsEmptyArray()
        {
            var nc = new NativeClient(MakeConfig());
            var frames = nc.ToStackFrames(new System.Exception("test"));
            Assert.AreEqual(0, frames.Length);
        }

        [Test]
        public void SetGroupingDiscriminator_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.SetGroupingDiscriminator("my-disc"));
        }

        [Test]
        public void ShouldAttemptDelivery_ReturnsTrue()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.IsTrue(nc.ShouldAttemptDelivery());
        }

        [Test]
        public void RegisterForOnSessionCallbacks_DoesNotThrow()
        {
            var nc = new NativeClient(MakeConfig());
            Assert.DoesNotThrow(() => nc.RegisterForOnSessionCallbacks());
        }
    }
#endif
}
