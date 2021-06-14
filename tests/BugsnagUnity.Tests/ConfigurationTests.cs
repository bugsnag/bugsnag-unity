using NUnit.Framework;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace BugsnagUnity.Payload.Tests
{
    [TestFixture]
    class ConfigurationTests
    {
        [Test]
        public void DefaultConfigurationValues()
        {
            var config = new Configuration("foo");
            Assert.IsTrue(config.ReportUncaughtExceptionsAsHandled);
            Assert.IsTrue(config.AutoNotify);
            Assert.IsTrue(config.AutoCaptureSessions);
            Assert.AreEqual("production", config.ReleaseStage);
            Assert.AreEqual("https://notify.bugsnag.com/", config.Endpoint.ToString());
            Assert.AreEqual("https://sessions.bugsnag.com/", config.SessionEndpoint.ToString());
            Assert.AreEqual("foo", config.ApiKey);
        }
    }
}
