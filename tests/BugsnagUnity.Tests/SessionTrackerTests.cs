using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
    [TestClass]
    public class SessionTrackerTests
    {
        private SessionTracker Tracker { get; set; }

        public SessionTrackerTests()
        {
            Client client = new Client(new NativeClient(new Configuration("api-key")));
            Tracker = new SessionTracker(client);
            Assert.IsNotNull(Tracker);
        }

        /**
        * Verifies that a session can be resumed after it is stopped
        */
        [TestMethod]
        public void ResumeFromStoppedSession()
        {
            Tracker.StartSession();
            var originalSession = Tracker.CurrentSession;
            Assert.IsNotNull(originalSession);

            Tracker.PauseSession();
            Assert.IsNull(Tracker.CurrentSession);

            Assert.IsTrue(Tracker.ResumeSession());
            Assert.IsTrue(SessionsAreTheSame(originalSession, Tracker.CurrentSession));
        }

        /**
        * Verifies that the previous session is resumed when calling SessionTracker.ResumeSession
        */
        [TestMethod]
        public void ResumeWithNoStoppedSession()
        {
            Tracker.StartSession();
            Tracker.PauseSession();
            Assert.IsTrue(Tracker.ResumeSession());
            Assert.IsFalse(Tracker.ResumeSession());
        }

        /**
        * Verifies that a new session can be created after the previous one is stopped
        */
        [TestMethod]
        public void StartNewAfterStoppedSession()
        {
            Tracker.StartSession();
            var originalSession = Tracker.CurrentSession;

            Tracker.PauseSession();
            Tracker.StartSession();
            Assert.AreNotEqual(originalSession, Tracker.CurrentSession);
        }

        /**
        * Verifies that calling SessionTracker.ResumeSession multiple times only starts one session
        */
        [TestMethod]
        public void MultipleResumesHaveNoEffect()
        {
            Tracker.StartSession();
            var original = Tracker.CurrentSession;
            Tracker.PauseSession();

            Assert.IsTrue(Tracker.ResumeSession());
            Assert.IsTrue(SessionsAreTheSame(original, Tracker.CurrentSession));

            Assert.IsFalse(Tracker.ResumeSession());
            Assert.IsTrue(SessionsAreTheSame(original, Tracker.CurrentSession));
        }

        /**
        * Verifies that calling SessionTracker.StopSession multiple times only stops one session
        */
        [TestMethod]
        public void MultipleStopsHaveNoEffect()
        {
            Tracker.StartSession();
            Assert.IsNotNull(Tracker.CurrentSession);

            Tracker.PauseSession();
            Assert.IsNull(Tracker.CurrentSession);

            Tracker.PauseSession();
            Assert.IsNull(Tracker.CurrentSession);
        }

        private bool SessionsAreTheSame(Session original, Session other)
        {
            return original.Id == other.Id
                && original.StartedAt == other.StartedAt;
        }
    }
}
