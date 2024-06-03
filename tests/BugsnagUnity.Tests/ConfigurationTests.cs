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
            config.MaximumBreadcrumbs = 501;
            Assert.AreEqual(config.MaximumBreadcrumbs, 100);
            config.MaximumBreadcrumbs = -1;
            Assert.AreEqual(config.MaximumBreadcrumbs, 100);
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

        [Test]
        public void RedactedKeysTest()
        {
            var config = new Configuration("foo");
            
            // // Default redacted keys
            Assert.IsTrue(config.KeyIsRedacted("password"));
            Assert.IsFalse(config.KeyIsRedacted("username"));

            var config2 = new Configuration("foo");

            // Custom redacted keys
            config2.RedactedKeys = new string[] { "secret", "token" };
            Assert.IsTrue(config2.KeyIsRedacted("secret"));
            Assert.IsTrue(config2.KeyIsRedacted("token"));
            Assert.IsFalse(config2.KeyIsRedacted("password"));

            var config3 = new Configuration("foo");

            // Regex pattern keys
            config3.RedactedKeys = new string[] { ".*_key$", "^token_.*" };
            Assert.IsTrue(config3.KeyIsRedacted("api_key"));
            Assert.IsTrue(config3.KeyIsRedacted("token_value"));
            Assert.IsFalse(config3.KeyIsRedacted("password"));
        }

        [Test]
        public void DiscardedClassesTest()
        {
            var config = new Configuration("foo");

            // No discard classes by default
            Assert.IsFalse(config.ErrorClassIsDiscarded("System.Exception"));


            var config2 = new Configuration("foo");

            // Adding discard classes
            config2.DiscardClasses = new string[] { "System.Exception", "System.NullReferenceException" };
            Assert.IsTrue(config2.ErrorClassIsDiscarded("System.Exception"));
            Assert.IsTrue(config2.ErrorClassIsDiscarded("System.NullReferenceException"));
            Assert.IsFalse(config2.ErrorClassIsDiscarded("System.ArgumentException"));

            var config3 = new Configuration("foo");

            // // Regex pattern discard classes
            config3.DiscardClasses = new string[] { "^System\\..*Exception$", ".*ReferenceException" };
            Assert.IsTrue(config3.ErrorClassIsDiscarded("System.Exception"));
            Assert.IsTrue(config3.ErrorClassIsDiscarded("System.NullReferenceException"));
            Assert.IsTrue(config3.ErrorClassIsDiscarded("CustomReferenceException"));
            Assert.IsFalse(config3.ErrorClassIsDiscarded("ArgumentError"));
        }
    }
}
