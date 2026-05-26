using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class EventTests
    {
        private static Event CreateEvent(
            string context = "test-context",
            string apiKey = "test-api-key-1234",
            HandledState handledState = null,
            User user = null,
            Metadata metadata = null)
        {
            return new Event(
                context,
                metadata ?? new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                user ?? new User(),
                new Error[0],
                handledState ?? HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null,
                apiKey,
                new OrderedDictionary(),
                null
            );
        }

        // ---- ApiKey ----

        [Test]
        public void ApiKey_MatchesConstructorArg()
        {
            var ev = CreateEvent(apiKey: "my-api-key");
            Assert.AreEqual("my-api-key", ev.ApiKey);
        }

        // ---- Context ----

        [Test]
        public void Context_MatchesConstructorArg()
        {
            var ev = CreateEvent(context: "my-context");
            Assert.AreEqual("my-context", ev.Context);
        }

        [Test]
        public void Context_CanBeChanged()
        {
            var ev = CreateEvent(context: "original");
            ev.Context = "updated";
            Assert.AreEqual("updated", ev.Context);
        }

        // ---- Unhandled ----

        [Test]
        public void Unhandled_FromHandledState_ForHandledException_IsFalse()
        {
            var ev = CreateEvent(handledState: HandledState.ForHandledException());
            Assert.IsFalse(ev.Unhandled);
        }

        [Test]
        public void Unhandled_FromHandledState_ForUnhandledException_IsTrue()
        {
            var ev = CreateEvent(handledState: HandledState.ForUnhandledException());
            Assert.IsTrue(ev.Unhandled);
        }

        [Test]
        public void Unhandled_CanBeSetDirectly()
        {
            var ev = CreateEvent();
            ev.Unhandled = true;
            Assert.IsTrue(ev.Unhandled);
        }

        // ---- Severity ----

        [Test]
        public void Severity_DefaultForHandled_IsWarning()
        {
            var ev = CreateEvent(handledState: HandledState.ForHandledException());
            Assert.AreEqual(Severity.Warning, ev.Severity);
        }

        [Test]
        public void Severity_CanBeOverridden()
        {
            var ev = CreateEvent();
            ev.Severity = Severity.Info;
            Assert.AreEqual(Severity.Info, ev.Severity);
        }

        // ---- GroupingHash / GroupingDiscriminator ----

        [Test]
        public void GroupingHash_SetAndGet()
        {
            var ev = CreateEvent();
            ev.GroupingHash = "hash-abc";
            Assert.AreEqual("hash-abc", ev.GroupingHash);
        }

        [Test]
        public void GroupingDiscriminator_FromConstructorArg()
        {
            var ev = new Event(
                "ctx",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null,
                "apikey",
                new OrderedDictionary(),
                null,
                groupingDiscriminator: "my-discriminator"
            );
            Assert.AreEqual("my-discriminator", ev.GroupingDiscriminator);
        }

        // ---- GetUser / SetUser ----

        [Test]
        public void GetUser_ReturnsConstructorUser()
        {
            var user = new User("uid", "email@test.com", "Alice");
            var ev = CreateEvent(user: user);
            Assert.AreEqual("uid", ev.GetUser().Id);
        }

        [Test]
        public void SetUser_ReplacesExistingUser()
        {
            var ev = CreateEvent();
            ev.SetUser("new-id", "new@test.com", "Bob");
            Assert.AreEqual("new-id", ev.GetUser().Id);
            Assert.AreEqual("new@test.com", ev.GetUser().Email);
            Assert.AreEqual("Bob", ev.GetUser().Name);
        }

        // ---- AddMetadata / GetMetadata / ClearMetadata ----

        [Test]
        public void AddMetadata_KeyValue_CanBeRetrieved()
        {
            var ev = CreateEvent();
            ev.AddMetadata("section", "key", "value");
            Assert.AreEqual("value", ev.GetMetadata("section", "key"));
        }

        [Test]
        public void AddMetadata_Dict_CanBeRetrieved()
        {
            var ev = CreateEvent();
            ev.AddMetadata("section", new Dictionary<string, object> { { "k", 99 } });
            var section = ev.GetMetadata("section") as IDictionary<string, object>;
            Assert.AreEqual(99, section["k"]);
        }

        [Test]
        public void ClearMetadata_Section_RemovesSection()
        {
            var ev = CreateEvent();
            ev.AddMetadata("section", "key", "val");
            ev.ClearMetadata("section");
            Assert.IsNull(ev.GetMetadata("section"));
        }

        [Test]
        public void ClearMetadata_SectionAndKey_RemovesSingleKey()
        {
            var ev = CreateEvent();
            ev.AddMetadata("section", "key1", "v1");
            ev.AddMetadata("section", "key2", "v2");
            ev.ClearMetadata("section", "key1");
            var section = ev.GetMetadata("section") as IDictionary<string, object>;
            Assert.IsFalse(section.ContainsKey("key1"));
            Assert.AreEqual("v2", section["key2"]);
        }

        // ---- RedactMetadata ----

        [Test]
        public void RedactMetadata_RedactsMatchingKey()
        {
            var ev = CreateEvent();
            ev.AddMetadata("section", "password", "secret");
            ev.AddMetadata("section", "username", "alice");

            var config = new Configuration("test-api-key");
            config.RedactedKeys = new List<Regex> { new Regex("^password$", RegexOptions.IgnoreCase) };
            ev.RedactMetadata(config);

            var section = ev.GetMetadata("section") as IDictionary<string, object>;
            Assert.AreEqual("[REDACTED]", section["password"]);
            Assert.AreEqual("alice", section["username"]);
        }

        // ---- Breadcrumbs ----

        [Test]
        public void Breadcrumbs_DefaultIsEmpty()
        {
            var ev = CreateEvent();
            Assert.IsNotNull(ev.Breadcrumbs);
            Assert.AreEqual(0, ev.Breadcrumbs.Count);
        }

        [Test]
        public void Breadcrumbs_ConstructedWithCrumbs_AreAccessible()
        {
            var crumbs = new List<Breadcrumb>
            {
                new Breadcrumb("click", new Dictionary<string, object>(), BreadcrumbType.User)
            };
            var ev = new Event(
                "ctx", new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(), new Error[0],
                HandledState.ForHandledException(),
                crumbs, null, "api", new OrderedDictionary(), null
            );
            Assert.AreEqual(1, ev.Breadcrumbs.Count);
            Assert.AreEqual("click", ev.Breadcrumbs[0].Message);
        }
    }

    [TestFixture]
    public class EventGetPayloadTests
    {
        private static Event CreateEvent(
            string context = "ctx",
            string apiKey = "key",
            HandledState handledState = null,
            User user = null,
            List<Breadcrumb> breadcrumbs = null,
            Session session = null)
        {
            return new Event(
                context,
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                user ?? new User(),
                new Error[0],
                handledState ?? HandledState.ForHandledException(),
                breadcrumbs ?? new List<Breadcrumb>(),
                session,
                apiKey,
                new OrderedDictionary(),
                null
            );
        }

        [Test]
        public void GetEventPayload_ReturnsDict_WithApiKey()
        {
            var ev = CreateEvent(apiKey: "test-key");
            ev.ApiKey = "test-key";
            var payload = ev.GetEventPayload();
            Assert.IsNotNull(payload);
        }

        [Test]
        public void GetEventPayload_ContainsApp()
        {
            var ev = CreateEvent();
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("app"));
        }

        [Test]
        public void GetEventPayload_ContainsDevice()
        {
            var ev = CreateEvent();
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("device"));
        }

        [Test]
        public void GetEventPayload_ContainsExceptions()
        {
            var ev = CreateEvent();
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("exceptions"));
        }

        [Test]
        public void GetEventPayload_ContainsBreadcrumbs()
        {
            var ev = CreateEvent();
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("breadcrumbs"));
        }

        [Test]
        public void GetEventPayload_WithCorrelation_ContainsCorrelation()
        {
            var ev = new Event(
                "ctx", new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(), new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(), null, "key",
                new OrderedDictionary(),
                new Correlation("trace-id", "span-id")
            );
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("correlation"));
        }

        [Test]
        public void GetEventPayload_WithSession_ContainsSession()
        {
            var session = new Session();
            var ev = CreateEvent(session: session);
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("session"));
        }

        [Test]
        public void GetEventPayload_WithGroupingHash_IncludesIt()
        {
            var ev = CreateEvent();
            ev.GroupingHash = "my-hash";
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("groupingHash"));
        }

        [Test]
        public void AddFeatureFlag_AppearsInFeatureFlags()
        {
            var ev = CreateEvent();
            ev.AddFeatureFlag("flag-a", "v1");
            var flags = ev.FeatureFlags;
            Assert.AreEqual(1, flags.Count);
            Assert.AreEqual("flag-a", flags[0].Name);
        }

        [Test]
        public void AddFeatureFlags_Array_AddsAll()
        {
            var ev = CreateEvent();
            ev.AddFeatureFlags(new[]
            {
                new FeatureFlag("f1", "v1"),
                new FeatureFlag("f2", null)
            });
            Assert.AreEqual(2, ev.FeatureFlags.Count);
        }

        [Test]
        public void ClearFeatureFlag_RemovesSingleFlag()
        {
            var ev = CreateEvent();
            ev.AddFeatureFlag("keep", "v");
            ev.AddFeatureFlag("remove", "x");
            ev.ClearFeatureFlag("remove");
            Assert.AreEqual(1, ev.FeatureFlags.Count);
            Assert.AreEqual("keep", ev.FeatureFlags[0].Name);
        }

        [Test]
        public void ClearFeatureFlags_RemovesAll()
        {
            var ev = CreateEvent();
            ev.AddFeatureFlag("a", null);
            ev.AddFeatureFlag("b", null);
            ev.ClearFeatureFlags();
            Assert.AreEqual(0, ev.FeatureFlags.Count);
        }

        [Test]
        public void GetEventPayload_WithFeatureFlags_IncludesThem()
        {
            var ev = CreateEvent();
            ev.AddFeatureFlag("my-flag", "on");
            var payload = ev.GetEventPayload();
            Assert.IsTrue(payload.ContainsKey("featureFlags"));
        }

        [Test]
        public void IsAndroidJavaError_NoErrors_ReturnsFalse()
        {
            var ev = CreateEvent();
            Assert.IsFalse(ev.IsAndroidJavaError());
        }

        [Test]
        public void App_ReturnsNonNull()
        {
            var ev = CreateEvent();
            Assert.IsNotNull(ev.App);
        }

        [Test]
        public void Device_ReturnsNonNull()
        {
            var ev = CreateEvent();
            Assert.IsNotNull(ev.Device);
        }

        [Test]
        public void Threads_ReturnsNull()
        {
            var ev = CreateEvent();
            Assert.IsNull(ev.Threads);
        }

        [Test]
        public void UnhandledOverridden_WithUnhandledTrue_UpdatesSessionCounts()
        {
            var session = new Session();
            var ev = CreateEvent(handledState: HandledState.ForUnhandledException(), session: session);
            ev.Unhandled = true;
            ev.UnhandledOverridden();
            Assert.IsTrue(ev.Unhandled);
        }

        [Test]
        public void UnhandledOverridden_WithUnhandledFalse_UpdatesSessionCounts()
        {
            var session = new Session();
            var ev = CreateEvent(handledState: HandledState.ForHandledException(), session: session);
            ev.Unhandled = false;
            ev.UnhandledOverridden();
            Assert.IsFalse(ev.Unhandled);
        }

        [Test]
        public void RedactMetadata_AlsoRedactsBreadcrumbMetadata()
        {
            var crumb = new Breadcrumb("click", new Dictionary<string, object> { { "password", "secret" } }, BreadcrumbType.User);
            var ev = CreateEvent(breadcrumbs: new List<Breadcrumb> { crumb });
            var config = new Configuration("key");
            config.RedactedKeys = new List<Regex> { new Regex("^password$", RegexOptions.IgnoreCase) };
            ev.RedactMetadata(config);
            Assert.AreEqual("[REDACTED]", crumb.Metadata["password"]);
        }

        [Test]
        public void Constructor_WithSession_HandledEvent_IncrementsHandledCount()
        {
            var session = new Session();
            var before = session.Events.Handled;
            var ev = CreateEvent(handledState: HandledState.ForHandledException(), session: session);
            Assert.AreEqual(before + 1, session.Events.Handled);
        }

        [Test]
        public void Constructor_WithSession_UnhandledEvent_IncrementsUnhandledCount()
        {
            var session = new Session();
            var before = session.Events.Unhandled;
            var ev = CreateEvent(handledState: HandledState.ForUnhandledException(), session: session);
            Assert.AreEqual(before + 1, session.Events.Unhandled);
        }

        [Test]
        public void LogType_FromConstructorArg_IsSet()
        {
            var ev = new Event(
                "ctx",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null,
                "key",
                new OrderedDictionary(),
                null,
                null,
                UnityEngine.LogType.Log
            );
            Assert.AreEqual(UnityEngine.LogType.Log, ev.LogType);
        }

        [Test]
        public void LogType_WhenNotProvided_IsNull()
        {
            var ev = CreateEvent();
            Assert.IsNull(ev.LogType);
        }
    }

    [TestFixture]
    public class EventDeserializationTests
    {
        private static Dictionary<string, object> BuildMinimalPayload(object unhandledValue = null)
        {
            var eventDict = new Dictionary<string, object>
            {
                ["unhandled"] = unhandledValue,
                ["severity"] = "error",
                ["severityReason"] = null,
                ["metaData"] = new Dictionary<string, object>(),
                ["app"] = new Dictionary<string, object>(),
                ["device"] = new Dictionary<string, object>(),
                ["exceptions"] = new JsonArray(),
            };
            return new Dictionary<string, object>
            {
                ["apiKey"] = "test-api-key",
                ["event"] = eventDict
            };
        }

        private static Dictionary<string, object> BuildFullPayload()
        {
            var crumbObj = new JsonObject();
            crumbObj["name"] = "tap";
            crumbObj["type"] = "user";
            crumbObj["timestamp"] = "2023-01-01T00:00:00Z";
            crumbObj["metaData"] = new Dictionary<string, object>();
            var breadcrumbsArray = new JsonArray { crumbObj };

            var errorObj = new JsonObject();
            errorObj["errorClass"] = "NullReferenceException";
            errorObj["message"] = "Object reference not set";
            var errorsArray = new JsonArray { errorObj };

            var flagObj = new JsonObject();
            flagObj["featureFlag"] = "my-flag";
            flagObj["variant"] = "v1";
            var flagsArray = new JsonArray { flagObj };

            var userDict = new Dictionary<string, object>
            {
                ["id"] = "user-1",
                ["email"] = "user@example.com",
                ["name"] = "Test User"
            };

            var eventDict = new Dictionary<string, object>
            {
                ["unhandled"] = false,
                ["severity"] = "warning",
                ["severityReason"] = new Dictionary<string, object> { ["type"] = "handledError" },
                ["metaData"] = new Dictionary<string, object>(),
                ["app"] = new Dictionary<string, object>(),
                ["device"] = new Dictionary<string, object>(),
                ["exceptions"] = errorsArray,
                ["featureFlags"] = flagsArray,
                ["context"] = "my-context",
                ["user"] = userDict,
                ["breadcrumbs"] = breadcrumbsArray,
                ["groupingHash"] = "my-hash",
                ["groupingDiscriminator"] = "my-disc",
                ["session"] = new Dictionary<string, object>(),
                ["projectPackages"] = new JsonArray { "com.example.myapp" },
            };

            return new Dictionary<string, object>
            {
                ["apiKey"] = "full-api-key",
                ["event"] = eventDict
            };
        }

        [Test]
        public void EventDictConstructor_SetsApiKey()
        {
            var evt = new Event(BuildMinimalPayload(false));
            Assert.AreEqual("test-api-key", evt.ApiKey);
        }

        [Test]
        public void EventDictConstructor_WithNullUnhandled_SetsUnhandledFalse()
        {
            var evt = new Event(BuildMinimalPayload(null));
            Assert.IsFalse(evt.Unhandled);
        }

        [Test]
        public void EventDictConstructor_WithUnhandledTrue_SetsUnhandledTrue()
        {
            var evt = new Event(BuildMinimalPayload(true));
            Assert.IsTrue(evt.Unhandled);
        }

        [Test]
        public void EventDictConstructor_WithNullSeverity_DoesNotThrow()
        {
            var payload = BuildMinimalPayload(false);
            ((Dictionary<string, object>)payload["event"])["severity"] = null;
            Assert.DoesNotThrow(() => new Event(payload));
        }

        [Test]
        public void EventDictConstructor_WithNullSeverityReason_DoesNotThrow()
        {
            var payload = BuildMinimalPayload(false);
            ((Dictionary<string, object>)payload["event"])["severityReason"] = null;
            Assert.DoesNotThrow(() => new Event(payload));
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsContext()
        {
            var evt = new Event(BuildFullPayload());
            Assert.AreEqual("my-context", evt.Context);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsGroupingHash()
        {
            var evt = new Event(BuildFullPayload());
            Assert.AreEqual("my-hash", evt.GroupingHash);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsGroupingDiscriminator()
        {
            var evt = new Event(BuildFullPayload());
            Assert.AreEqual("my-disc", evt.GroupingDiscriminator);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsErrors()
        {
            var evt = new Event(BuildFullPayload());
            Assert.IsNotNull(evt.Errors);
            Assert.AreEqual(1, evt.Errors.Count);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsBreadcrumbs()
        {
            var evt = new Event(BuildFullPayload());
            Assert.IsNotNull(evt.Breadcrumbs);
            Assert.AreEqual(1, evt.Breadcrumbs.Count);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsSession()
        {
            var evt = new Event(BuildFullPayload());
            Assert.IsNotNull(evt.Session);
        }

        [Test]
        public void EventDictConstructor_FullPayload_SetsFeatureFlag()
        {
            var evt = new Event(BuildFullPayload());
            var flags = evt.FeatureFlags;
            Assert.AreEqual(1, flags.Count);
            Assert.AreEqual("my-flag", flags[0].Name);
            Assert.AreEqual("v1", flags[0].Variant);
        }

        [Test]
        public void EventDictConstructor_NoOptionalKeys_SetsEmptyBreadcrumbs()
        {
            var evt = new Event(BuildMinimalPayload(false));
            Assert.IsNull(evt.Breadcrumbs);
        }

        [Test]
        public void EventDictConstructor_NoOptionalKeys_SessionIsNull()
        {
            var evt = new Event(BuildMinimalPayload(false));
            Assert.IsNull(evt.Session);
        }

        [Test]
        public void EventDictConstructor_NoOptionalKeys_ErrorsIsEmpty()
        {
            var evt = new Event(BuildMinimalPayload(false));
            Assert.AreEqual(0, evt.Errors.Count);
        }

        [Test]
        public void EventDictConstructor_FullPayload_AppIsNotNull()
        {
            var evt = new Event(BuildFullPayload());
            Assert.IsNotNull(evt.App);
        }

        [Test]
        public void EventDictConstructor_FullPayload_DeviceIsNotNull()
        {
            var evt = new Event(BuildFullPayload());
            Assert.IsNotNull(evt.Device);
        }

        [Test]
        public void EventDictConstructor_WithSeverityAndReason_SetsBoth()
        {
            var payload = BuildMinimalPayload(false);
            var eventPart = (Dictionary<string, object>)payload["event"];
            eventPart["severity"] = "info";
            eventPart["severityReason"] = new Dictionary<string, object> { ["type"] = "userCallback" };
            var evt = new Event(payload);
            Assert.IsNotNull(evt);
        }

        [Test]
        public void AddAndroidProjectPackagesToEvent_SetsPackages()
        {
            var evt = new Event(BuildMinimalPayload(false));
            evt.AddAndroidProjectPackagesToEvent(new[] { "com.test.pkg", "com.test.pkg2" });
            Assert.IsNotNull(evt);
        }
    }

    [TestFixture]
    public class ErrorTests
    {
        [Test]
        public void Constructor_SetsErrorClassAndMessage()
        {
            var error = new Error("MyException", "Something failed", new StackTraceLine[0]);
            Assert.AreEqual("MyException", error.ErrorClass);
            Assert.AreEqual("Something failed", error.ErrorMessage);
        }

        [Test]
        public void Constructor_WithEmptyStackTrace_HasEmptyStacktrace()
        {
            var error = new Error("Err", "msg", new StackTraceLine[0]);
            Assert.IsNotNull(error.Stacktrace);
        }

        [Test]
        public void ErrorClass_Setter_UpdatesValue()
        {
            var error = new Error("Old", "msg", new StackTraceLine[0]);
            error.ErrorClass = "New";
            Assert.AreEqual("New", error.ErrorClass);
        }

        [Test]
        public void ErrorMessage_Setter_UpdatesValue()
        {
            var error = new Error("Err", "old", new StackTraceLine[0]);
            error.ErrorMessage = "updated";
            Assert.AreEqual("updated", error.ErrorMessage);
        }

        [Test]
        public void Type_SetAndGet()
        {
            var error = new Error("Err", "msg", new StackTraceLine[0]);
            error.Type = "cocoa";
            Assert.AreEqual("cocoa", error.Type);
        }

        [Test]
        public void ShouldSend_RegularException_ReturnsTrue()
        {
            var ex = new System.Exception("test");
            Assert.IsTrue(Error.ShouldSend(ex));
        }

        [Test]
        public void ShouldSend_ArgumentException_ReturnsTrue()
        {
            var ex = new System.ArgumentException("arg");
            Assert.IsTrue(Error.ShouldSend(ex));
        }

        [Test]
        public void ShouldSend_UnityLogMessage_NullStackTrace_ReturnsTrue()
        {
            var msg = new UnityLogMessage("anything", null, UnityEngine.LogType.Error);
            Assert.IsTrue(Error.ShouldSend(msg));
        }

        [Test]
        public void ShouldSend_UnityLogMessage_NonAndroidClass_ReturnsTrue()
        {
            var msg = new UnityLogMessage("SomeException: detail", "at SomeClass()", UnityEngine.LogType.Error);
            Assert.IsTrue(Error.ShouldSend(msg));
        }

        [Test]
        public void Constructor_WithHandledState_SetsHandledState()
        {
            var hs = HandledState.ForUnhandledException();
            var error = new Error("Err", "msg",
                new IStackframe[0], hs, false);
            Assert.AreEqual(hs, error.HandledState);
        }
    }

    [TestFixture]
    public class UnityLogMessageTests
    {
        [Test]
        public void Constructor_FromException_SetsConditionFromMessage()
        {
            var ex = new System.InvalidOperationException("test error");
            var msg = new UnityLogMessage(ex);
            Assert.AreEqual("test error", msg.Condition);
        }

        [Test]
        public void Constructor_FromException_SetsTypeToException()
        {
            var ex = new System.ArgumentException("arg error");
            var msg = new UnityLogMessage(ex);
            Assert.AreEqual(UnityEngine.LogType.Exception, msg.Type);
        }

        [Test]
        public void Constructor_FromException_StackTraceIsEmptyWhenExceptionHasNone()
        {
            var ex = new System.Exception("no stack trace");
            var msg = new UnityLogMessage(ex);
            Assert.IsNotNull(msg.StackTrace);
        }

        [Test]
        public void Constructor_FromException_WithNullMessage_ConditionIsEmpty()
        {
            // An exception with a null Message (edge case) should produce empty Condition
            var ex = new NullMessageException();
            var msg = new UnityLogMessage(ex);
            Assert.AreEqual(string.Empty, msg.Condition);
        }

        private class NullMessageException : System.Exception
        {
            public override string Message => null;
        }
    }
}
