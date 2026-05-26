using NUnit.Framework;
using BugsnagUnity;
using UnityEngine;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class PolyfillStringTests
    {
        [Test]
        public void IsNullOrWhiteSpace_Null_ReturnsTrue()
        {
            Assert.IsTrue(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace(null));
        }

        [Test]
        public void IsNullOrWhiteSpace_Empty_ReturnsTrue()
        {
            Assert.IsTrue(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace(""));
        }

        [Test]
        public void IsNullOrWhiteSpace_Whitespace_ReturnsTrue()
        {
            Assert.IsTrue(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace("   "));
        }

        [Test]
        public void IsNullOrWhiteSpace_NonWhitespace_ReturnsFalse()
        {
            Assert.IsFalse(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace("hello"));
        }

        [Test]
        public void IsNullOrWhiteSpace_PaddedString_ReturnsFalse()
        {
            Assert.IsFalse(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace(" a "));
        }

        [Test]
        public void IsNullOrWhiteSpace_SingleTab_ReturnsTrue()
        {
            Assert.IsTrue(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace("\t"));
        }

        [Test]
        public void IsNullOrWhiteSpace_NewlineOnly_ReturnsTrue()
        {
            Assert.IsTrue(BugsnagUnity.Polyfills.String.IsNullOrWhiteSpace("\n"));
        }
    }

    [TestFixture]
    public class LogTypeExtensionsTests
    {
        [Test]
        public void IsGreaterThanOrEqualTo_SameType_ReturnsTrue()
        {
            Assert.IsTrue(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Log, LogType.Log));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_WarningGreaterThanLog_ReturnsTrue()
        {
            Assert.IsTrue(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Warning, LogType.Log));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_ExceptionGreaterThanError_ReturnsTrue()
        {
            Assert.IsTrue(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Exception, LogType.Error));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_ErrorEqualError_ReturnsTrue()
        {
            Assert.IsTrue(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Error, LogType.Error));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_LogLessThanWarning_ReturnsFalse()
        {
            Assert.IsFalse(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Log, LogType.Warning));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_ErrorLessThanException_ReturnsFalse()
        {
            Assert.IsFalse(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Error, LogType.Exception));
        }

        [Test]
        public void IsGreaterThanOrEqualTo_AssertBetweenWarningAndError_CorrectOrdering()
        {
            Assert.IsTrue(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Assert, LogType.Warning));
            Assert.IsFalse(LogTypeExtensions.IsGreaterThanOrEqualTo(LogType.Assert, LogType.Error));
        }
    }

    [TestFixture]
    public class UpTimeClockTests
    {
        [Test]
        public void Elapsed_ReturnsNonNegativeTimeSpan()
        {
            var elapsed = UptimeClock.Elapsed;
            Assert.GreaterOrEqual(elapsed.TotalSeconds, 0);
        }

        [Test]
        public void Elapsed_CalledTwice_SecondGreaterOrEqual()
        {
            var first = UptimeClock.Elapsed;
            var second = UptimeClock.Elapsed;
            Assert.GreaterOrEqual(second, first);
        }
    }
}
