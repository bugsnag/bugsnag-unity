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
  }
}
