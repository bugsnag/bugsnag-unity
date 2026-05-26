using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class HandledStateTests
    {
        [Test]
        public void ForUnhandledException_SeverityIsError_UnhandledIsTrue()
        {
            var state = HandledState.ForUnhandledException();
            Assert.AreEqual(Severity.Error, state.Severity);
            Assert.AreEqual(true, state["unhandled"]);
        }

        [Test]
        public void ForHandledException_SeverityIsWarning_UnhandledIsFalse()
        {
            var state = HandledState.ForHandledException();
            Assert.AreEqual(Severity.Warning, state.Severity);
            Assert.AreEqual(false, state["unhandled"]);
        }

        [Test]
        public void ForLoggedException_SeverityIsError_UnhandledIsFalse()
        {
            var state = HandledState.ForLoggedException();
            Assert.AreEqual(Severity.Error, state.Severity);
            Assert.AreEqual(false, state["unhandled"]);
        }

        [Test]
        public void ForUserSpecifiedSeverity_Info_SeverityIsInfo_UnhandledIsFalse()
        {
            var state = HandledState.ForUserSpecifiedSeverity(Severity.Info);
            Assert.AreEqual(Severity.Info, state.Severity);
            Assert.AreEqual(false, state["unhandled"]);
        }

        [Test]
        public void ForUserSpecifiedSeverity_Warning_SeverityIsWarning()
        {
            var state = HandledState.ForUserSpecifiedSeverity(Severity.Warning);
            Assert.AreEqual(Severity.Warning, state.Severity);
        }

        [Test]
        public void ForCallbackSpecifiedSeverity_InheritsHandledFromPrevious_Unhandled()
        {
            var previous = HandledState.ForUnhandledException(); // unhandled = true
            var state = HandledState.ForCallbackSpecifiedSeverity(Severity.Info, previous);
            Assert.AreEqual(Severity.Info, state.Severity);
            // previous was unhandled → ForCallbackSpecifiedSeverity inherits handled=false → unhandled=true
            Assert.AreEqual(true, state["unhandled"]);
        }

        [Test]
        public void ForCallbackSpecifiedSeverity_InheritsHandledFromPrevious_Handled()
        {
            var previous = HandledState.ForHandledException(); // handled = true
            var state = HandledState.ForCallbackSpecifiedSeverity(Severity.Error, previous);
            Assert.AreEqual(Severity.Error, state.Severity);
            Assert.AreEqual(false, state["unhandled"]);
        }

        [Test]
        public void ForUnityLogMessage_SetsCorrectSeverityAndIsHandled()
        {
            var state = HandledState.ForUnityLogMessage(Severity.Warning);
            Assert.AreEqual(Severity.Warning, state.Severity);
            Assert.AreEqual(false, state["unhandled"]);
        }

        [Test]
        public void SeverityStringInPayload_Error()
        {
            var state = HandledState.ForUnhandledException();
            Assert.AreEqual("error", state["severity"]);
        }

        [Test]
        public void SeverityStringInPayload_Warning()
        {
            var state = HandledState.ForHandledException();
            Assert.AreEqual("warning", state["severity"]);
        }

        [Test]
        public void SeverityStringInPayload_Info()
        {
            var state = HandledState.ForUserSpecifiedSeverity(Severity.Info);
            Assert.AreEqual("info", state["severity"]);
        }
    }
}
