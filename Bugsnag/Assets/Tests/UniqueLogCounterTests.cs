using NUnit.Framework;
using System.Threading;
using UnityEngine;
using BugsnagUnity;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class UniqueLogCounterTests
    {
        [Test]
        public void ShouldNotSendDuplicateMessages()
        {
            var counter = new UniqueLogThrottle(new Configuration("foo"));

            var message1 = new UnityLogMessage("", "", LogType.Error);
            var message2 = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message1);
            Assert.IsFalse(counter.ShouldSend(message2));
        }

        [Test]
        public void SendsSingleMessage()
        {
            var counter = new UniqueLogThrottle(new Configuration("foo"));

            var message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }

        [Test]
        public void FlushesCorrectly()
        {
            var configuration = new Configuration("foo");
            var counter = new UniqueLogThrottle(configuration);

            var message = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message);

            // Sleep for the duration specified in the configuration (convert seconds to milliseconds)
            Thread.Sleep(configuration.SecondsPerUniqueLog);

            message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }
    }
}