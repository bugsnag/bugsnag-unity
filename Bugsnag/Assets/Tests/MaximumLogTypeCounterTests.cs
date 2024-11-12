using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using BugsnagUnity;
using System.Threading;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class MaximumLogTypeCounterTests
    {
        [Test]
        public void SendsSingleMessage()
        {
            Dictionary<LogType, int> maximumTypePerTimePeriod =
              new Dictionary<LogType, int> { { LogType.Error, 5 } };

            var configuration = new Configuration("foo")
            {
                MaximumTypePerTimePeriod = maximumTypePerTimePeriod
            };

            var counter = new MaximumLogTypeCounter(configuration);

            var message = new UnityLogMessage("", "", UnityEngine.LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }

        [Test]
        public void ShouldNotSendOverLimitMessages()
        {
            Dictionary<LogType, int> maximumTypePerTimePeriod =
              new Dictionary<LogType, int> { { LogType.Error, 1 } };

            var configuration = new Configuration("foo")
            {
                MaximumTypePerTimePeriod = maximumTypePerTimePeriod
            };

            var counter = new MaximumLogTypeCounter(configuration);

            var message1 = new UnityLogMessage("", "", LogType.Error);
            var message2 = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message1);
            Assert.IsFalse(counter.ShouldSend(message2));
        }

        [Test]
        public void ShouldSendUnderTheLimit()
        {
            Dictionary<LogType, int> maximumTypePerTimePeriod =
              new Dictionary<LogType, int> { { LogType.Error, 5 } };

            var configuration = new Configuration("foo")
            {
                MaximumTypePerTimePeriod = maximumTypePerTimePeriod
            };

            var counter = new MaximumLogTypeCounter(configuration);

            var message1 = new UnityLogMessage("", "", LogType.Error);
            var message2 = new UnityLogMessage("", "", LogType.Error);
            var message3 = new UnityLogMessage("", "", LogType.Error);
            var message4 = new UnityLogMessage("", "", LogType.Error);
            var message5 = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message1));
            Assert.IsTrue(counter.ShouldSend(message2));
            Assert.IsTrue(counter.ShouldSend(message3));
            Assert.IsTrue(counter.ShouldSend(message4));
            Assert.IsTrue(counter.ShouldSend(message5));
        }

        [Test]
        public void DontTrackCertainLogType()
        {
            Dictionary<LogType, int> maximumTypePerTimePeriod =
              new Dictionary<LogType, int>();

            var configuration = new Configuration("foo")
            {
                MaximumTypePerTimePeriod = maximumTypePerTimePeriod
            };

            var counter = new MaximumLogTypeCounter(configuration);

            var message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }

        [Test]
        public void FlushesCorrectly()
        {
            Dictionary<LogType, int> maximumTypePerTimePeriod =
              new Dictionary<LogType, int> { { LogType.Error, 1 } };

            var configuration = new Configuration("foo")
            {
                MaximumTypePerTimePeriod = maximumTypePerTimePeriod,
                MaximumLogsTimePeriod = TimeSpan.FromSeconds(2),
            };

            var counter = new MaximumLogTypeCounter(configuration);

            var message = new UnityLogMessage("", "", LogType.Error);

            counter.ShouldSend(message);

            // Convert TimeSpan to seconds for WaitForSeconds
            Thread.Sleep(configuration.MaximumLogsTimePeriod);

            message = new UnityLogMessage("", "", LogType.Error);

            Assert.IsTrue(counter.ShouldSend(message));
        }
    }
}