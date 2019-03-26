using NUnit.Framework;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
  class TestConfig : AbstractConfiguration
  {
    internal TestConfig(string apiKey) : base() {
      SetupDefaults(apiKey);
    }
  }

  [TestFixture]
  class ConfigurationTests
  {
    [Test]
    public void ReportUncaughtExceptionsAsHandledIsDefault()
    {
      var config = new TestConfig("foo");
      Assert.IsTrue(config.ReportUncaughtExceptionsAsHandled);
    }

    // [Test]
    // public void AnrEnabledByDefault()
    // {
    //   var config = new TestConfig("foo");
    //   Assert.IsTrue(config.DetectAnrs);
    // }


    [Test]
    public void AlteringAnrThresholdMs()
    {
      var config = new TestConfig("foo");
      Assert.AreEqual(5000, config.AnrThresholdMs);

      config.AnrThresholdMs = 99;
      Assert.AreEqual(1000, config.AnrThresholdMs);
    }
  }
}
