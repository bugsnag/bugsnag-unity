using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Bugsnag.Unity.Tests
{
  [TestFixture]
  public class UniqueLogCounterTests
  {
    class TestConfiguration : IConfiguration
    {
      public TimeSpan MaximumLogsTimePeriod { get; set; }

      public Dictionary<LogType, int> MaximumTypePerTimePeriod { get; set; }

      public TimeSpan UniqueLogsTimePeriod { get; set; }

      public string ApiKey { get; set; }

      public int MaximumBreadcrumbs { get; set; }

      public string ReleaseStage { get; set; }

      public string AppVersion { get; set; }

      public Uri Endpoint { get; set; }

      public string PayloadVersion { get; set; }

      public Uri SessionEndpoint { get; set; }

      public string SessionPayloadVersion { get; set; }

      public string Context { get; set; }

      public LogType NotifyLevel { get; set; }

      public bool AutoNotify { get; set; }
    }

    [Test]
    public void ShouldNotSendDuplicateMessages()
    {
      var counter = new UniqueLogThrottle(new TestConfiguration());

      var message1 = new UnityLogMessage("", "", LogType.Error);
      var message2 = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message1);
      Assert.False(counter.ShouldSend(message2));
    }

    [Test]
    public void SendsSingleMessage()
    {
      var counter = new UniqueLogThrottle(new TestConfiguration());

      var message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }

    [Test]
    public void FlushesCorrectly()
    {
      var configuration = new TestConfiguration();
      var counter = new UniqueLogThrottle(configuration);

      var message = new UnityLogMessage("", "", LogType.Error);

      counter.ShouldSend(message);

      Thread.Sleep(configuration.UniqueLogsTimePeriod);

      message = new UnityLogMessage("", "", LogType.Error);

      Assert.True(counter.ShouldSend(message));
    }
  }
}
