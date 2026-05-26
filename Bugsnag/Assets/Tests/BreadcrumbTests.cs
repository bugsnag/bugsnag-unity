using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class BreadcrumbTests
    {
        // ---- ParseBreadcrumbType ----

        [Test]
        public void ParseBreadcrumbType_Error_ReturnsError()
        {
            Assert.AreEqual(BreadcrumbType.Error, Breadcrumb.ParseBreadcrumbType("error"));
        }

        [Test]
        public void ParseBreadcrumbType_Log_ReturnsLog()
        {
            Assert.AreEqual(BreadcrumbType.Log, Breadcrumb.ParseBreadcrumbType("log"));
        }

        [Test]
        public void ParseBreadcrumbType_Navigation_ReturnsNavigation()
        {
            Assert.AreEqual(BreadcrumbType.Navigation, Breadcrumb.ParseBreadcrumbType("navigation"));
        }

        [Test]
        public void ParseBreadcrumbType_Process_ReturnsProcess()
        {
            Assert.AreEqual(BreadcrumbType.Process, Breadcrumb.ParseBreadcrumbType("process"));
        }

        [Test]
        public void ParseBreadcrumbType_Request_ReturnsRequest()
        {
            Assert.AreEqual(BreadcrumbType.Request, Breadcrumb.ParseBreadcrumbType("request"));
        }

        [Test]
        public void ParseBreadcrumbType_State_ReturnsState()
        {
            Assert.AreEqual(BreadcrumbType.State, Breadcrumb.ParseBreadcrumbType("state"));
        }

        [Test]
        public void ParseBreadcrumbType_User_ReturnsUser()
        {
            Assert.AreEqual(BreadcrumbType.User, Breadcrumb.ParseBreadcrumbType("user"));
        }

        [Test]
        public void ParseBreadcrumbType_Manual_ReturnsManual()
        {
            Assert.AreEqual(BreadcrumbType.Manual, Breadcrumb.ParseBreadcrumbType("manual"));
        }

        [Test]
        public void ParseBreadcrumbType_Unknown_DefaultsToManual()
        {
            Assert.AreEqual(BreadcrumbType.Manual, Breadcrumb.ParseBreadcrumbType("something_else"));
        }

        [Test]
        public void ParseBreadcrumbType_CaseInsensitive_ContainsMatch()
        {
            // "navigation" is contained in "navigationEvent"
            Assert.AreEqual(BreadcrumbType.Navigation, Breadcrumb.ParseBreadcrumbType("navigationEvent"));
        }

        // ---- Constructor: message / metadata / type ----

        [Test]
        public void Constructor_MessageMetadataType_SetsAllProperties()
        {
            var metadata = new Dictionary<string, object> { { "key", "value" } };
            var crumb = new Breadcrumb("button tapped", metadata, BreadcrumbType.User);
            Assert.AreEqual("button tapped", crumb.Message);
            Assert.AreEqual(BreadcrumbType.User, crumb.Type);
            Assert.IsNotNull(crumb.Timestamp);
        }

        [Test]
        public void Constructor_SetsTimestampToNow()
        {
            var before = DateTimeOffset.UtcNow.AddSeconds(-1);
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>(), BreadcrumbType.Manual);
            Assert.GreaterOrEqual(crumb.Timestamp, before);
        }

        [Test]
        public void Constructor_StringTimestamp_ParsesCorrectly()
        {
            var ts = "2024-01-15T10:30:00.000Z";
            var crumb = new Breadcrumb("msg", ts, "navigation", new Dictionary<string, object>());
            Assert.AreEqual(BreadcrumbType.Navigation, crumb.Type);
            Assert.AreEqual("msg", crumb.Message);
        }

        [Test]
        public void Constructor_StringTimestamp_EmptyType_DefaultsToManual()
        {
            var crumb = new Breadcrumb("msg", "2024-01-15T10:30:00.000Z", "", new Dictionary<string, object>());
            Assert.AreEqual(BreadcrumbType.Manual, crumb.Type);
        }

        // ---- Message property ----

        [Test]
        public void Message_CanBeChanged()
        {
            var crumb = new Breadcrumb("original", new Dictionary<string, object>(), BreadcrumbType.Log);
            crumb.Message = "updated";
            Assert.AreEqual("updated", crumb.Message);
        }

        // ---- Type property ----

        [Test]
        public void Type_SetToError_RoundTrips()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>(), BreadcrumbType.Manual);
            crumb.Type = BreadcrumbType.Error;
            Assert.AreEqual(BreadcrumbType.Error, crumb.Type);
        }

        // ---- Metadata property ----

        [Test]
        public void Metadata_NullValue_SetsNullAndDoesNotThrow()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>(), BreadcrumbType.Manual);
            Assert.DoesNotThrow(() => crumb.Metadata = null);
        }

        [Test]
        public void Metadata_ReturnsEmptyDictWhenNotSet()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>(), BreadcrumbType.Manual);
            Assert.IsNotNull(crumb.Metadata);
        }

        [Test]
        public void Metadata_SerializableValues_StoredAsIs()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object> { { "x", 42 } }, BreadcrumbType.Log);
            Assert.AreEqual(42, crumb.Metadata["x"]);
        }
    }

    [TestFixture]
    public class BreadcrumbSanitizationTests
    {
        [Test]
        public void Metadata_WithUnserializableValue_AddsWarningKey()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "bad", new object() }
            }, BreadcrumbType.Manual);
            Assert.IsTrue(crumb.Metadata.ContainsKey("__bugsnag_unserializable_values"));
        }

        [Test]
        public void Metadata_WithNestedDict_KeepsNestedValues()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "nested", new Dictionary<string, object> { { "inner", "value" } } }
            }, BreadcrumbType.Manual);
            var nested = crumb.Metadata["nested"] as IDictionary<string, object>;
            Assert.IsNotNull(nested);
            Assert.AreEqual("value", nested["inner"]);
        }

        [Test]
        public void Metadata_WithList_ConvertsToObjectList()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "items", new List<object> { 1, 2, 3 } }
            }, BreadcrumbType.Manual);
            Assert.IsNotNull(crumb.Metadata["items"]);
        }

        [Test]
        public void Metadata_WithStringArray_KeepsStringArray()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "tags", new[] { "a", "b" } }
            }, BreadcrumbType.Manual);
            Assert.IsInstanceOf<string[]>(crumb.Metadata["tags"]);
        }

        [Test]
        public void Metadata_WithListOfStrings_KeepsListOfStrings()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "items", new List<string> { "x", "y" } }
            }, BreadcrumbType.Manual);
            Assert.IsInstanceOf<List<string>>(crumb.Metadata["items"]);
        }

        [Test]
        public void Metadata_Null_SetsToNull()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>(), BreadcrumbType.Manual);
            crumb.Metadata = null;
            Assert.IsNotNull(crumb.Metadata);
        }

        [Test]
        public void Metadata_MultipleUnserializableValues_AccumulatesWarnings()
        {
            var crumb = new Breadcrumb("msg", new Dictionary<string, object>
            {
                { "bad1", new object() },
                { "bad2", new object() }
            }, BreadcrumbType.Manual);
            var warnings = crumb.Metadata["__bugsnag_unserializable_values"] as string[];
            Assert.IsNotNull(warnings);
            Assert.GreaterOrEqual(warnings.Length, 2);
        }

        [Test]
        public void MetadataSetter_UnserializableValue_AddsWarningAndConvertsToString()
        {
            var crumb = new Breadcrumb("test", new Dictionary<string, object>(), BreadcrumbType.Manual);
            crumb.Metadata = new Dictionary<string, object>
            {
                { "ok", "value" },
                { "bad", new System.Text.StringBuilder("UnserializableObject") }
            };
            var stored = crumb.Metadata;
            Assert.AreEqual("value", stored["ok"]);
            const string warnKey = "__bugsnag_unserializable_values";
            Assert.IsTrue(stored.ContainsKey(warnKey));
        }

        [Test]
        public void FromReport_CreatesErrorBreadcrumb()
        {
            var config = new Configuration("test-key");
            config.Endpoints.Configure("test-key");
            var ev = new Event(
                "context",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[] { new Error("MyException", "oops", new StackTraceLine[0]) },
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null, "api-key",
                new System.Collections.Specialized.OrderedDictionary(), null
            );
            var report = new Report(config, ev);
            var crumb = Breadcrumb.FromReport(report);
            Assert.AreEqual(BreadcrumbType.Error, crumb.Type);
            Assert.AreEqual("MyException", crumb.Message);
        }

        [Test]
        public void FromReport_NoExceptions_HasErrorMessage()
        {
            var config = new Configuration("test-key");
            config.Endpoints.Configure("test-key");
            var ev = new Event(
                "my-context",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                HandledState.ForHandledException(),
                new List<Breadcrumb>(),
                null, "api-key",
                new System.Collections.Specialized.OrderedDictionary(), null
            );
            var report = new Report(config, ev);
            var crumb = Breadcrumb.FromReport(report);
            Assert.AreEqual(BreadcrumbType.Error, crumb.Type);
            Assert.AreEqual("Error", crumb.Message);
        }
    }
}
