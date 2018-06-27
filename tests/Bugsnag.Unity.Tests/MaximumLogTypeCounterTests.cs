using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Bugsnag.Unity.Tests
{
  [TestFixture]
  public class MaximumLogTypeCounterTests
  {
    [Test]
    public void SendsSingleMessage()
    {
      Dictionary<LogType, int> maximumTypePerTimePeriod =
        new Dictionary<LogType, int> { { LogType.Error, 5 } };

      var configuration = new TestConfiguration
      {
        MaximumTypePerTimePeriod = maximumTypePerTimePeriod
      };

      var counter = new MaximumLogTypeCounter(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void ShouldNotSendOverLimitMessages()
    {
      Dictionary<LogType, int> maximumTypePerTimePeriod =
        new Dictionary<LogType, int> { { LogType.Error, 1 } };

      var configuration = new TestConfiguration
      {
        MaximumTypePerTimePeriod = maximumTypePerTimePeriod
      };

      var counter = new MaximumLogTypeCounter(configuration);

      var message1 = new UnityLogMessage("", "", LogType.Error);
      var message2 = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message1);
      Assert.False(counter.ShouldSend(message2));
    }

    [Test]
    public void ShouldSendUnderTheLimit()
    {
      Dictionary<LogType, int> maximumTypePerTimePeriod =
        new Dictionary<LogType, int> { { LogType.Error, 5 } };

      var configuration = new TestConfiguration
      {
        MaximumTypePerTimePeriod = maximumTypePerTimePeriod
      };

      var counter = new MaximumLogTypeCounter(configuration);

      var message1 = new UnityLogMessage("", "", LogType.Error);
      var message2 = new UnityLogMessage("", "", LogType.Error);
      var message3 = new UnityLogMessage("", "", LogType.Error);
      var message4 = new UnityLogMessage("", "", LogType.Error);
      var message5 = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message1));
      Assert.True(counter.ShouldSend(message2));
      Assert.True(counter.ShouldSend(message3));
      Assert.True(counter.ShouldSend(message4));
      Assert.True(counter.ShouldSend(message5));
    }

    [Test]
    public void DontTrackCertainLogType()
    {
      Dictionary<LogType, int> maximumTypePerTimePeriod =
        new Dictionary<LogType, int>();

      var configuration = new TestConfiguration
      {
        MaximumTypePerTimePeriod = maximumTypePerTimePeriod
      };

      var counter = new MaximumLogTypeCounter(configuration);


      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void FlushesCorrectly()
    {
      Dictionary<LogType, int> maximumTypePerTimePeriod =
        new Dictionary<LogType, int> { { LogType.Error, 1 } };

      var configuration = new TestConfiguration
      {
        MaximumTypePerTimePeriod = maximumTypePerTimePeriod
      };

      var counter = new MaximumLogTypeCounter(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message);

      Thread.Sleep(configuration.MaximumLogsTimePeriod);

      message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }
  }
}
