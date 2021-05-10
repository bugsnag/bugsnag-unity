using NUnit.Framework;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Tests
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
      Assert.False(counter.ShouldSend(message2));
    }

    [Test]
    public void SendsSingleMessage()
    {
      var counter = new UniqueLogThrottle(new Configuration("foo"));

      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void FlushesCorrectly()
    {
      var configuration = new Configuration("foo");
      var counter = new UniqueLogThrottle(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message);

      Thread.Sleep(configuration.UniqueLogsTimePeriod);

      message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }
  }
}
