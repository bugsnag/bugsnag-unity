using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using BugsnagUnity;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class SessionTests
    {
        [Test]
        public void Constructor_SetsNonNullNonEmptyId()
        {
            var session = new Session();
            Assert.IsNotNull(session.Id);
            Assert.IsNotEmpty(session.Id);
        }

        [Test]
        public void Constructor_SetsStartedAt_ToNow()
        {
            var before = DateTimeOffset.Now;
            var session = new Session();
            Assert.IsNotNull(session.StartedAt);
            Assert.GreaterOrEqual(session.StartedAt, before);
        }

        [Test]
        public void DefaultHandledCount_IsZero()
        {
            var session = new Session();
            Assert.AreEqual(0, session.HandledCount());
        }

        [Test]
        public void DefaultUnhandledCount_IsZero()
        {
            var session = new Session();
            Assert.AreEqual(0, session.UnhandledCount());
        }

        [Test]
        public void Constructor_WithInitialCounts_SetsCountsCorrectly()
        {
            var session = new Session(DateTimeOffset.Now, 3, 2);
            Assert.AreEqual(3, session.HandledCount());
            Assert.AreEqual(2, session.UnhandledCount());
        }

        [Test]
        public void Copy_PreservesIdAndCounts()
        {
            var session = new Session(DateTimeOffset.Now, 4, 1);
            var copy = session.Copy();
            Assert.AreEqual(session.Id, copy.Id);
            Assert.AreEqual(4, copy.HandledCount());
            Assert.AreEqual(1, copy.UnhandledCount());
        }

        [Test]
        public void Copy_IsIndependentFromOriginal()
        {
            var session = new Session(DateTimeOffset.Now, 1, 0);
            var copy = session.Copy();
            // Modifying the copy's events should not be visible in original
            copy.Events.UpdateHandledCount(true);
            Assert.AreEqual(1, session.HandledCount());
            Assert.AreEqual(2, copy.HandledCount());
        }

        [Test]
        public void SetUser_GetUser_RoundTrip()
        {
            var session = new Session();
            session.SetUser("id1", "email@test.com", "Alice");
            var user = session.GetUser();
            Assert.IsNotNull(user);
            Assert.AreEqual("id1", user.Id);
            Assert.AreEqual("email@test.com", user.Email);
            Assert.AreEqual("Alice", user.Name);
        }

        [Test]
        public void GetUser_BeforeSet_ReturnsNull()
        {
            var session = new Session();
            Assert.IsNull(session.GetUser());
        }

        [Test]
        public void SessionWithProvidedGuid_UsesProvidedGuid()
        {
            var guid = "fixed-guid-123";
            var session = new Session(guid, DateTimeOffset.Now, 0, 0);
            Assert.AreEqual(guid, session.Id);
        }
    }

    [TestFixture]
    public class SessionEventsTests
    {
        [Test]
        public void InitialHandledCount_IsZero()
        {
            var session = new Session();
            Assert.AreEqual(0, session.Events.Handled);
        }

        [Test]
        public void InitialUnhandledCount_IsZero()
        {
            var session = new Session();
            Assert.AreEqual(0, session.Events.Unhandled);
        }

        [Test]
        public void UpdateHandledCount_Increment()
        {
            var session = new Session();
            session.Events.UpdateHandledCount(true);
            Assert.AreEqual(1, session.Events.Handled);
        }

        [Test]
        public void UpdateHandledCount_Decrement()
        {
            var session = new Session();
            session.Events.UpdateHandledCount(true);
            session.Events.UpdateHandledCount(false);
            Assert.AreEqual(0, session.Events.Handled);
        }

        [Test]
        public void UpdateUnhandledCount_Increment()
        {
            var session = new Session();
            session.Events.UpdateUnhandledCount(true);
            Assert.AreEqual(1, session.Events.Unhandled);
        }

        [Test]
        public void UpdateUnhandledCount_Decrement()
        {
            var session = new Session();
            session.Events.UpdateUnhandledCount(true);
            session.Events.UpdateUnhandledCount(false);
            Assert.AreEqual(0, session.Events.Unhandled);
        }

        [Test]
        public void HandledAndUnhandled_AreIndependent()
        {
            var session = new Session();
            session.Events.UpdateHandledCount(true);
            session.Events.UpdateHandledCount(true);
            session.Events.UpdateUnhandledCount(true);
            Assert.AreEqual(2, session.Events.Handled);
            Assert.AreEqual(1, session.Events.Unhandled);
        }
    }

    [TestFixture]
    public class SessionAddExceptionTests
    {
        private static Report MakeReport(bool handled)
        {
            var config = new Configuration("test-api-key");
            config.Endpoints.Configure("test-api-key");
            var handledState = handled
                ? HandledState.ForHandledException()
                : HandledState.ForUnhandledException();
            var ev = new Event(
                "ctx",
                new Metadata(),
                new AppWithState(new Dictionary<string, object>()),
                new DeviceWithState(new Dictionary<string, object>()),
                new User(),
                new Error[0],
                handledState,
                new List<Breadcrumb>(),
                null,
                "test-api-key",
                new OrderedDictionary(),
                null
            );
            return new Report(config, ev);
        }

        [Test]
        public void AddException_HandledReport_IncrementsHandledCount()
        {
            var session = new Session();
            var before = session.HandledCount();
            session.AddException(MakeReport(handled: true));
            Assert.AreEqual(before + 1, session.HandledCount());
        }

        [Test]
        public void AddException_UnhandledReport_IncrementsUnhandledCount()
        {
            var session = new Session();
            var before = session.UnhandledCount();
            session.AddException(MakeReport(handled: false));
            Assert.AreEqual(before + 1, session.UnhandledCount());
        }

        [Test]
        public void AddException_HandledReport_DoesNotChangeUnhandledCount()
        {
            var session = new Session();
            var before = session.UnhandledCount();
            session.AddException(MakeReport(handled: true));
            Assert.AreEqual(before, session.UnhandledCount());
        }

        [Test]
        public void AddException_UnhandledReport_DoesNotChangeHandledCount()
        {
            var session = new Session();
            var before = session.HandledCount();
            session.AddException(MakeReport(handled: false));
            Assert.AreEqual(before, session.HandledCount());
        }
    }
}
