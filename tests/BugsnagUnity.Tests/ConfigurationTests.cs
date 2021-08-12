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
            Assert.IsTrue(config.AutoDetectErrors);
            Assert.IsTrue(config.AutoTrackSessions);
            Assert.AreEqual("production", config.ReleaseStage);
            Assert.AreEqual("https://notify.bugsnag.com/", config.Endpoints.Notify.ToString());
            Assert.AreEqual("https://sessions.bugsnag.com/", config.Endpoints.Session.ToString());
            Assert.AreEqual("foo", config.ApiKey);
        }


        [Test]
        public void MaxBreadcrumbsLimit()
        {
            var config = new Configuration("foo");
            config.MaximumBreadcrumbs = 101;
            Assert.AreEqual(config.MaximumBreadcrumbs, 25);
            config.MaximumBreadcrumbs = -1;
            Assert.AreEqual(config.MaximumBreadcrumbs, 25);
            config.MaximumBreadcrumbs = 20;
            Assert.AreEqual(config.MaximumBreadcrumbs, 20);
        }
    }
}
