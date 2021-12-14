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
            Assert.IsTrue(config.ReportExceptionLogsAsHandled);
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

        [Test]
        public void CloneTest()
        {
            var original = new Configuration("foo");

            original.MaximumBreadcrumbs = 1;
            original.ReleaseStage = "1";
            original.SetUser("1","1","1");

            var clone = original.Clone();

            //int check
            Assert.AreEqual(original.MaximumBreadcrumbs, clone.MaximumBreadcrumbs);
            clone.MaximumBreadcrumbs = 2;
            Assert.AreEqual(1, original.MaximumBreadcrumbs);
            Assert.AreEqual( 2, clone.MaximumBreadcrumbs);

            //string check
            clone.ReleaseStage = "2";
            Assert.AreNotEqual(original.ReleaseStage, clone.ReleaseStage);


            //user check
            clone.SetUser("2", "2", "2");
            Assert.AreEqual("1", original.GetUser().Name);
            Assert.AreEqual("2", clone.GetUser().Name);
        }


        [Test]
        public void EndpointValidation()
        {
            var config = new Configuration("foo");

            Assert.IsTrue(config.Endpoints.IsValid);

            config.Endpoints.Notify = new System.Uri("https://www.richIsCool.com/");

            Assert.IsFalse(config.Endpoints.IsValid);

            config.Endpoints.Session = new System.Uri("https://www.richIsSuperCool.com/");

            Assert.IsTrue(config.Endpoints.IsValid);

            config.Endpoints.Notify = null;

            Assert.IsFalse(config.Endpoints.IsValid);
        }
    }
}
