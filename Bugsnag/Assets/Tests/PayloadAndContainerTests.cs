using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class PayloadContainerTests
    {
        // PayloadContainer is internal, but App/Device extend it and expose it.
        // We exercise PayloadContainer through App which uses it directly.

        [Test]
        public void Add_KeyValue_CanBeRetrievedViaProperty()
        {
            var app = new App(new Dictionary<string, object>());
            app.Id = "my-id";
            Assert.AreEqual("my-id", app.Id);
        }

        [Test]
        public void Add_Null_RemovesExistingKey()
        {
            var app = new App(new Dictionary<string, object>());
            app.Id = "initial";
            app.Id = null;
            Assert.IsNull(app.Id);
        }

        [Test]
        public void Get_MissingKey_ReturnsNull()
        {
            var device = new Device(new Dictionary<string, object>());
            Assert.IsNull(device.BrowserName);
        }

        [Test]
        public void Add_Dictionary_AddsAllEntries()
        {
            var device = new Device(new Dictionary<string, object>
            {
                { "model", "Pixel" },
                { "osName", "Android" }
            });
            Assert.AreEqual("Pixel", device.Model);
            Assert.AreEqual("Android", device.OsName);
        }
    }

    [TestFixture]
    public class PayloadExtensionsTests
    {
        [Test]
        public void AddToPayload_NullValue_RemovesKey()
        {
            var dict = new Dictionary<string, object> { { "key", "value" } };
            dict.AddToPayload("key", (object)null);
            Assert.IsFalse(dict.ContainsKey("key"));
        }

        [Test]
        public void AddToPayload_NonNullString_AddsKey()
        {
            var dict = new Dictionary<string, object>();
            dict.AddToPayload("key", (object)"hello");
            Assert.AreEqual("hello", dict["key"]);
        }

        [Test]
        public void AddToPayload_WhitespaceString_RemovesKey()
        {
            var dict = new Dictionary<string, object> { { "key", "old" } };
            dict.AddToPayload("key", (object)"   ");
            Assert.IsFalse(dict.ContainsKey("key"));
        }

        [Test]
        public void AddToPayload_EmptyString_RemovesKey()
        {
            var dict = new Dictionary<string, object> { { "key", "old" } };
            dict.AddToPayload("key", (object)"");
            Assert.IsFalse(dict.ContainsKey("key"));
        }

        [Test]
        public void AddToPayload_WhitespaceString_NoExistingKey_DoesNotAdd()
        {
            var dict = new Dictionary<string, object>();
            dict.AddToPayload("key", (object)"  ");
            Assert.IsFalse(dict.ContainsKey("key"));
        }

        [Test]
        public void AddToPayload_IntValue_Adds()
        {
            var dict = new Dictionary<string, object>();
            dict.AddToPayload("count", (object)42);
            Assert.AreEqual(42, dict["count"]);
        }

        [Test]
        public void Get_ExistingKey_ReturnsValue()
        {
            var dict = new Dictionary<string, object> { { "k", "v" } };
            Assert.AreEqual("v", dict.Get("k"));
        }

        [Test]
        public void Get_MissingKey_ReturnsNull()
        {
            var dict = new Dictionary<string, object>();
            Assert.IsNull(dict.Get("missing"));
        }
    }

    [TestFixture]
    public class EnabledErrorTypesTests
    {
        [Test]
        public void DefaultConstructor_AllErrorTypesEnabled()
        {
            var e = new EnabledErrorTypes();
            Assert.IsTrue(e.ANRs);
            Assert.IsTrue(e.AppHangs);
            Assert.IsTrue(e.OOMs);
            Assert.IsTrue(e.Crashes);
            Assert.IsTrue(e.ThermalKills);
            Assert.IsTrue(e.UnityLog);
        }

        [Test]
        public void ANRs_CanBeDisabled()
        {
            var e = new EnabledErrorTypes { ANRs = false };
            Assert.IsFalse(e.ANRs);
            Assert.IsTrue(e.Crashes);
        }

        [Test]
        public void AllFields_CanBeSetToFalse()
        {
            var e = new EnabledErrorTypes
            {
                ANRs = false,
                AppHangs = false,
                OOMs = false,
                Crashes = false,
                ThermalKills = false,
                UnityLog = false
            };
            Assert.IsFalse(e.ANRs);
            Assert.IsFalse(e.AppHangs);
            Assert.IsFalse(e.OOMs);
            Assert.IsFalse(e.Crashes);
            Assert.IsFalse(e.ThermalKills);
            Assert.IsFalse(e.UnityLog);
        }
    }

    [TestFixture]
    public class NotifierInfoTests
    {
        [Test]
        public void Instance_IsNotNull()
        {
            Assert.IsNotNull(NotifierInfo.Instance);
        }

        [Test]
        public void Instance_ContainsName()
        {
            Assert.IsTrue(NotifierInfo.Instance.ContainsKey("name"));
            Assert.AreEqual(NotifierInfo.NotifierName, NotifierInfo.Instance["name"]);
        }

        [Test]
        public void Instance_ContainsUrl()
        {
            Assert.IsTrue(NotifierInfo.Instance.ContainsKey("url"));
            Assert.AreEqual(NotifierInfo.NotifierUrl, NotifierInfo.Instance["url"]);
        }

        [Test]
        public void Instance_ContainsVersion()
        {
            Assert.IsTrue(NotifierInfo.Instance.ContainsKey("version"));
        }

        [Test]
        public void NotifierName_IsExpected()
        {
            Assert.AreEqual("Unity Bugsnag Notifier", NotifierInfo.NotifierName);
        }

        [Test]
        public void NotifierUrl_IsExpected()
        {
            Assert.AreEqual("https://github.com/bugsnag/bugsnag-unity", NotifierInfo.NotifierUrl);
        }
    }

    [TestFixture]
    public class ReportTests
    {
        private static Configuration MakeConfig()
        {
            var config = new Configuration("test-api-key");
            config.Endpoints.Configure("test-api-key");
            return config;
        }

        private static Event MakeEvent(string apiKey = "test-api-key")
        {
            return new Event(
                "ctx",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null,
                apiKey,
                new OrderedDictionary(),
                null
            );
        }

        [Test]
        public void Constructor_SetsId_ToNonEmptyGuid()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            Assert.IsFalse(string.IsNullOrEmpty(report.Id));
        }

        [Test]
        public void Constructor_SetsEndpoint_FromConfig()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            Assert.IsNotNull(report.Endpoint);
            Assert.AreEqual("https://notify.bugsnag.com", report.Endpoint.ToString().TrimEnd('/'));
        }

        [Test]
        public void Constructor_SetsHeaders_WithApiKey()
        {
            var report = new Report(MakeConfig(), MakeEvent("my-api-key"));
            var header = Array.Find(report.Headers, h => h.Key == "Bugsnag-Api-Key");
            Assert.AreEqual("my-api-key", header.Value);
        }

        [Test]
        public void Constructor_SetsHeaders_WithPayloadVersion()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            var header = Array.Find(report.Headers, h => h.Key == "Bugsnag-Payload-Version");
            Assert.IsFalse(string.IsNullOrEmpty(header.Value));
        }

        [Test]
        public void Constructor_IsNotIgnored_ByDefault()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            Assert.IsFalse(report.Ignored);
        }

        [Test]
        public void Ignore_SetsIgnoredTrue()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            report.Ignore();
            Assert.IsTrue(report.Ignored);
        }

        [Test]
        public void PayloadType_IsEvent()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            Assert.AreEqual(PayloadType.Event, report.PayloadType);
        }

        [Test]
        public void GetSerialisablePayload_ContainsId()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("id"));
        }

        [Test]
        public void GetSerialisablePayload_ContainsApiKey()
        {
            var report = new Report(MakeConfig(), MakeEvent("my-api-key"));
            var payload = report.GetSerialisablePayload();
            Assert.AreEqual("my-api-key", payload["apiKey"]);
        }

        [Test]
        public void GetSerialisablePayload_ContainsNotifier()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("notifier"));
        }

        [Test]
        public void GetSerialisablePayload_ContainsEvent()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("event"));
        }

        [Test]
        public void ApplyEventsArray_AddsEventsKey()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            report.ApplyEventsArray();
            Assert.IsTrue(report.ContainsKey("events"));
        }

        [Test]
        public void IsHandled_ForHandledEvent_IsTrue()
        {
            var report = new Report(MakeConfig(), MakeEvent());
            Assert.IsTrue(report.IsHandled);
        }

        [Test]
        public void Context_MatchesEventContext()
        {
            var ev = MakeEvent();
            ev.Context = "my-context";
            var report = new Report(MakeConfig(), ev);
            Assert.AreEqual("my-context", report.Context);
        }

        [Test]
        public void Session_WhenEventHasSession_IsNotNull()
        {
            var session = new Session();
            var ev = new Event(
                "ctx",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                session,
                "test-api-key",
                new OrderedDictionary(),
                null
            );
            var report = new Report(MakeConfig(), ev);
            Assert.IsNotNull(report.Session);
        }

        [Test]
        public void DictConstructor_SetsId()
        {
            var original = new Report(MakeConfig(), MakeEvent());
            var payload = original.GetSerialisablePayload();
            var json = SerializeToJson(payload);
            var restoredDict = ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary();
            var restored = new Report(MakeConfig(), restoredDict);
            Assert.AreEqual(original.Id, restored.Id);
        }

        [Test]
        public void DictConstructor_SetsEndpoint()
        {
            var original = new Report(MakeConfig(), MakeEvent());
            var payload = original.GetSerialisablePayload();
            var json = SerializeToJson(payload);
            var restoredDict = ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary();
            var restored = new Report(MakeConfig(), restoredDict);
            Assert.IsNotNull(restored.Endpoint);
        }

        [Test]
        public void DictConstructor_SetsHeaders_WithApiKey()
        {
            var original = new Report(MakeConfig(), MakeEvent("my-api-key"));
            var payload = original.GetSerialisablePayload();
            var json = SerializeToJson(payload);
            var restoredDict = ((JsonObject)SimpleJson.DeserializeObject(json)).GetDictionary();
            var restored = new Report(MakeConfig(), restoredDict);
            var header = Array.Find(restored.Headers, h => h.Key == "Bugsnag-Api-Key");
            Assert.AreEqual("my-api-key", header.Value);
        }

        private static string SerializeToJson(object obj)
        {
            using var ms = new System.IO.MemoryStream();
            using var sw = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false));
            SimpleJson.SerializeObject(obj, sw);
            sw.Flush();
            return new System.Text.UTF8Encoding(false).GetString(ms.ToArray());
        }
    }

    [TestFixture]
    public class SessionReportTests
    {
        private static Configuration MakeConfig()
        {
            var config = new Configuration("test-api-key");
            config.Endpoints.Configure("test-api-key");
            return config;
        }

        private static Session MakeSession()
        {
            var session = new Session();
            session.App = new App(new Dictionary<string, object>());
            session.Device = new Device(new Dictionary<string, object>());
            session.SetUser("u1", "u@test.com", "User");
            return session;
        }

        [Test]
        public void Constructor_SetsId()
        {
            var session = MakeSession();
            var report = new SessionReport(MakeConfig(), session);
            Assert.IsFalse(string.IsNullOrEmpty(report.Id));
        }

        [Test]
        public void Constructor_SetsEndpoint()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            Assert.IsNotNull(report.Endpoint);
        }

        [Test]
        public void Constructor_SetsHeaders_WithApiKey()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            var header = Array.Find(report.Headers, h => h.Key == "Bugsnag-Api-Key");
            Assert.AreEqual("test-api-key", header.Value);
        }

        [Test]
        public void PayloadType_IsSession()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            Assert.AreEqual(PayloadType.Session, report.PayloadType);
        }

        [Test]
        public void GetSerialisablePayload_ContainsId()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("id"));
        }

        [Test]
        public void GetSerialisablePayload_ContainsNotifier()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("notifier"));
        }

        [Test]
        public void GetSerialisablePayload_ContainsApp()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("app"));
        }

        [Test]
        public void GetSerialisablePayload_ContainsSessions()
        {
            var report = new SessionReport(MakeConfig(), MakeSession());
            var payload = report.GetSerialisablePayload();
            Assert.IsTrue(payload.ContainsKey("sessions"));
        }

        [Test]
        public void DictConstructor_SetsId()
        {
            var original = new SessionReport(MakeConfig(), MakeSession());
            var payload = original.GetSerialisablePayload();
            var restored = new SessionReport(MakeConfig(), payload);
            Assert.AreEqual(original.Id, restored.Id);
        }

        [Test]
        public void DictConstructor_SetsEndpoint()
        {
            var original = new SessionReport(MakeConfig(), MakeSession());
            var payload = original.GetSerialisablePayload();
            var restored = new SessionReport(MakeConfig(), payload);
            Assert.IsNotNull(restored.Endpoint);
        }

        [Test]
        public void DictConstructor_PreservesNotifier()
        {
            var original = new SessionReport(MakeConfig(), MakeSession());
            var payload = original.GetSerialisablePayload();
            var restored = new SessionReport(MakeConfig(), payload);
            var restoredPayload = restored.GetSerialisablePayload();
            Assert.IsTrue(restoredPayload.ContainsKey("notifier"));
        }

        [Test]
        public void DictConstructor_PreservesApp()
        {
            var original = new SessionReport(MakeConfig(), MakeSession());
            var payload = original.GetSerialisablePayload();
            var restored = new SessionReport(MakeConfig(), payload);
            var restoredPayload = restored.GetSerialisablePayload();
            Assert.IsTrue(restoredPayload.ContainsKey("app"));
        }
    }
}
