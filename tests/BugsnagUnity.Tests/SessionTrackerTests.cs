using NUnit.Framework;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
  [TestFixture]
  class SessionTrackerTests
  {

    public SessionTracker Tracker { get; set; }

    public SessionTrackerTests()
    {
      Debug.unityLogger = new StubLogger();
      Client client = new Client(new NativeClient(new Configuration("api-key")));
      Tracker = new SessionTracker(client);
      Assert.IsNotNull(Tracker);
    }

     /**
     * Verifies that a session can be resumed after it is stopped
     */
    [Test]
    public void ResumeFromStoppedSession() {
      Tracker.StartSession();
      var originalSession = Tracker.CurrentSession;
      Assert.IsNotNull(originalSession);

      Tracker.StopSession();
      Assert.IsNull(Tracker.CurrentSession);

      Assert.IsTrue(Tracker.ResumeSession());
      Assert.AreEqual(originalSession, Tracker.CurrentSession);
    }

     /**
     * Verifies that the previous session is resumed when calling SessionTracker.ResumeSession
     */
    [Test]
    public void ResumeWithNoStoppedSession() {
      Tracker.StartSession();
      Tracker.StopSession();
      Assert.IsTrue(Tracker.ResumeSession());
      Assert.IsFalse(Tracker.ResumeSession());
    }

     /**
     * Verifies that a new session can be created after the previous one is stopped
     */
    [Test]
    public void StartNewAfterStoppedSession() {
      Tracker.StartSession();
      var originalSession = Tracker.CurrentSession;

      Tracker.StopSession();
      Tracker.StartSession();
      Assert.AreNotEqual(originalSession, Tracker.CurrentSession);
    }

     /**
     * Verifies that calling SessionTracker.ResumeSession multiple times only starts one session
     */
    [Test]
    public void MultipleResumesHaveNoEffect() {
      Tracker.StartSession();
      var original = Tracker.CurrentSession;
      Tracker.StopSession();

      Assert.IsTrue(Tracker.ResumeSession());
      Assert.AreEqual(original, Tracker.CurrentSession);

      Assert.IsFalse(Tracker.ResumeSession());
      Assert.AreEqual(original, Tracker.CurrentSession);
    }

     /**
     * Verifies that calling SessionTracker.StopSession multiple times only stops one session
     */
    [Test]
    public void MultipleStopsHaveNoEffect() {
      Tracker.StartSession();
      Assert.IsNotNull(Tracker.CurrentSession);

      Tracker.StopSession();
      Assert.IsNull(Tracker.CurrentSession);

      Tracker.StopSession();
      Assert.IsNull(Tracker.CurrentSession);
    }
  }
}
