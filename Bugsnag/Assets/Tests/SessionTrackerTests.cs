using System;
using System.Linq;
using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class SessionTrackerTests
    {
        private SessionTracker Tracker { get; set; }

        [SetUp]
        public void SetUp()
        {
            Client client = new Client(new NativeClient(new Configuration("api-key")));
            Tracker = new SessionTracker(client);
            Assert.IsNotNull(Tracker);
        }

        /// <summary>
        /// Verifies that a session can be resumed after it is stopped.
        /// </summary>
        [Test]
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

        /// <summary>
        /// Verifies that the previous session is resumed when calling SessionTracker.ResumeSession.
        /// </summary>
        [Test]
        public void ResumeWithNoStoppedSession()
        {
            Tracker.StartSession();
            Tracker.PauseSession();
            Assert.IsTrue(Tracker.ResumeSession());
            Assert.IsFalse(Tracker.ResumeSession());
        }

        /// <summary>
        /// Verifies that a new session can be created after the previous one is stopped.
        /// </summary>
        [Test]
        public void StartNewAfterStoppedSession()
        {
            Tracker.StartSession();
            var originalSession = Tracker.CurrentSession;

            Tracker.PauseSession();
            Tracker.StartSession();
            Assert.AreNotEqual(originalSession, Tracker.CurrentSession);
        }

        /// <summary>
        /// Verifies that calling SessionTracker.ResumeSession multiple times only starts one session.
        /// </summary>
        [Test]
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

        /// <summary>
        /// Verifies that calling SessionTracker.PauseSession multiple times only stops one session.
        /// </summary>
        [Test]
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