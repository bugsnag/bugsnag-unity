using System;
using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class GroupingDiscriminatorTests
    {
        private Configuration _config;

        [SetUp]
        public void SetUp()
        {
            _config = new Configuration("test-api-key");
        }

        [Test]
        public void EventGroupingDiscriminatorPropertyExists()
        {
            var @event = CreateTestEvent();
            
            // Test initial value is null
            Assert.IsNull(@event.GroupingDiscriminator);
            
            // Test setting and getting the value
            @event.GroupingDiscriminator = "test-discriminator";
            Assert.AreEqual("test-discriminator", @event.GroupingDiscriminator);
        }

        [Test]
        public void EventConstructorAcceptsGroupingDiscriminator()
        {
            var @event = CreateTestEventWithDiscriminator("constructor-discriminator");
            Assert.AreEqual("constructor-discriminator", @event.GroupingDiscriminator);
        }

        [Test]
        public void EventSerializationIncludesGroupingDiscriminator()
        {
            var @event = CreateTestEventWithDiscriminator("serialization-test");
            var payload = @event.GetEventPayload();
            
            Assert.IsTrue(payload.ContainsKey("groupingDiscriminator"));
            Assert.AreEqual("serialization-test", payload["groupingDiscriminator"]);
        }

        [Test]
        public void EventSerializationHandlesNullGroupingDiscriminator()
        {
            var @event = CreateTestEventWithDiscriminator(null);
            var payload = @event.GetEventPayload();
            
            Assert.IsTrue(payload.ContainsKey("groupingDiscriminator"));
            Assert.IsNull(payload["groupingDiscriminator"]);
        }

        [Test]
        public void ClientGroupingDiscriminatorGetterSetterWorks()
        {
            var nativeClient = new FallbackNativeClient(_config);
            var client = new Client(nativeClient);
            
            // Test initial value is null
            Assert.IsNull(client.GetGroupingDiscriminator());
            
            // Test setting and getting a value
            var previousValue = client.SetGroupingDiscriminator("client-test");
            Assert.IsNull(previousValue); // Should return null as previous value
            Assert.AreEqual("client-test", client.GetGroupingDiscriminator());
            
            // Test setting another value and returning previous
            previousValue = client.SetGroupingDiscriminator("client-test-2");
            Assert.AreEqual("client-test", previousValue);
            Assert.AreEqual("client-test-2", client.GetGroupingDiscriminator());
        }

        [Test]
        public void StaticBugsnagGroupingDiscriminatorProperty()
        {
            if (Bugsnag.IsStarted())
            {
                // Test setting and getting through static property
                Bugsnag.GroupingDiscriminator = "static-test";
                Assert.AreEqual("static-test", Bugsnag.GroupingDiscriminator);
                
                // Test setting to null
                Bugsnag.GroupingDiscriminator = null;
                Assert.IsNull(Bugsnag.GroupingDiscriminator);
            }
            else
            {
                // If Bugsnag is not started, we can't test the static property
                Assert.Ignore("Bugsnag not started - skipping static property test");
            }
        }

        private BugsnagUnity.Payload.Event CreateTestEvent()
        {
            return CreateTestEventWithDiscriminator(null);
        }

        private BugsnagUnity.Payload.Event CreateTestEventWithDiscriminator(string discriminator)
        {
            var metadata = new Metadata();
            var app = new AppWithState(_config);
            var device = new DeviceWithState(_config, "test-device-id");
            var user = new User("test-user-id", "test@example.com", "Test User");
            var errors = new Error[] { new Error("Test", "Test message", new StackTraceLine[0]) };
            var handledState = HandledState.ForUnhandledException();
            var breadcrumbs = new List<Breadcrumb>();
            var featureFlags = new OrderedDictionary();
            var correlation = new Correlation("","");

            return new BugsnagUnity.Payload.Event(
                "test-context",
                metadata,
                app,
                device,
                user,
                errors,
                handledState,
                breadcrumbs,
                null, // session
                "test-api-key",
                featureFlags,
                correlation,
                discriminator);
        }
    }

    // Simple mock implementation for testing purposes
    internal class FallbackNativeClient : INativeClient
    {
        public Configuration Configuration { get; }
        public IBreadcrumbs Breadcrumbs { get; }

        public FallbackNativeClient(Configuration configuration)
        {
            Configuration = configuration;
            Breadcrumbs = new Breadcrumbs(configuration);
        }

        public void PopulateApp(App app) { }
        public void PopulateAppWithState(AppWithState app) { }
        public void PopulateDevice(Device device) { }
        public void PopulateDeviceWithState(DeviceWithState device) { }
        public void StartSession() { }
        public void PauseSession() { }
        public bool ResumeSession() => false;
        public void UpdateSession(Session session) { }
        public Session GetCurrentSession() => null;
        public void SetUser(User user) { }
        public void PopulateUser(User user) { }
        public void SetContext(string context) { }
        public void MarkLaunchCompleted() { }
        public LastRunInfo GetLastRunInfo() => null;
        public void ClearNativeMetadata(string section) { }
        public void ClearNativeMetadata(string section, string key) { }
        public void AddNativeMetadata(string section, IDictionary<string, object> data) { }
        public IDictionary<string, object> GetNativeMetadata() => new Dictionary<string, object>();
        public StackTraceLine[] ToStackFrames(System.Exception exception) => new StackTraceLine[0];
        public bool ShouldAttemptDelivery() => true;
        public void RegisterForOnSessionCallbacks() { }
        public void AddFeatureFlag(string name, string variant = null) { }
        public void AddFeatureFlags(FeatureFlag[] featureFlags) { }
        public void ClearFeatureFlag(string name) { }
        public void ClearFeatureFlags() { }
        public void SetGroupingDiscriminator(string groupingDiscriminator) { }
    }
}
