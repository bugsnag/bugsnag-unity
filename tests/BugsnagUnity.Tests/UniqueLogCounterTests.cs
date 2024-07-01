using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Tests
{
    [TestClass]
    public class UniqueLogCounterTests
    {
        [TestMethod]
        public void ShouldNotSendDuplicateMessages()
        {
            var counter = new UniqueLogThrottle(new Configuration("foo"));

            var message1 = new UnityLogMessage("", "", LogType.Error);
            var message2 = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message1);
            Assert.IsFalse(counter.ShouldSend(message2));
        }

        [TestMethod]
        public void SendsSingleMessage()
        {
            var counter = new UniqueLogThrottle(new Configuration("foo"));

            var message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }

        [TestMethod]
        public void FlushesCorrectly()
        {
            var configuration = new Configuration("foo");
            var counter = new UniqueLogThrottle(configuration);

            var message = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message);

            Thread.Sleep(configuration.SecondsPerUniqueLog);

            message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }
    }
}
