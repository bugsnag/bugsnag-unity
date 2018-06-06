using NUnit.Framework;
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
      var counter = new MaximumLogTypeCounter(new Configuration("test"));

      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void ShouldNotSendOverLimitMessages()
    {
      Configuration configuration = new Configuration("test");
      configuration.MaximumTypePerTimePeriod[LogType.Error] = 1;
      var counter = new MaximumLogTypeCounter(configuration);

      var message1 = new UnityLogMessage("", "", LogType.Error);
      var message2 = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message1);
      Assert.False(counter.ShouldSend(message2));
    }

    [Test]
    public void ShouldSendUnderTheLimit()
    {
      Configuration configuration = new Configuration("test");
      configuration.MaximumTypePerTimePeriod[LogType.Error] = 5;
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
      Configuration configuration = new Configuration("test");
      configuration.MaximumTypePerTimePeriod.Remove(LogType.Error);
      var counter = new MaximumLogTypeCounter(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void FlushesCorrectly()
    {
      var configuration = new Configuration("test");
      configuration.MaximumTypePerTimePeriod[LogType.Error] = 1;
      var counter = new MaximumLogTypeCounter(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message);

      Thread.Sleep(configuration.MaximumLogsTimePeriod);

      message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }
  }
}
