using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

            // Default redacted keys
            Assert.IsTrue(config.KeyIsRedacted("user-password"));
            Assert.IsFalse(config.KeyIsRedacted("username"));

            var config2 = new Configuration("foo");

            // Custom redacted keys
            config2.RedactedKeys.Add(new Regex(".*secret.*", RegexOptions.IgnoreCase));
            config2.RedactedKeys.Add(new Regex(".*token.*", RegexOptions.IgnoreCase));
            Assert.IsTrue(config2.KeyIsRedacted("secret"));
            Assert.IsTrue(config2.KeyIsRedacted("token"));
            Assert.IsTrue(config2.KeyIsRedacted("password"));
            Assert.IsFalse(config2.KeyIsRedacted("app_id"));
        }

        [Test]
        public void DiscardedClassesTest()
        {
            var config = new Configuration("foo");

            // No discard classes by default
            Assert.IsFalse(config.ErrorClassIsDiscarded("System.Exception"));

            var config2 = new Configuration("foo");

            // Adding discard classes
            config2.DiscardClasses.Add(new Regex("^System\\.Exception$", RegexOptions.IgnoreCase));
            config2.DiscardClasses.Add(new Regex("^System\\.NullReferenceException$", RegexOptions.IgnoreCase));
            Assert.IsTrue(config2.ErrorClassIsDiscarded("System.Exception"));
            Assert.IsTrue(config2.ErrorClassIsDiscarded("System.NullReferenceException"));
            Assert.IsFalse(config2.ErrorClassIsDiscarded("System.ArgumentException"));
        }

        [Test]
        public void ThreadSafeCallbacksTest()
        {
            var config = new Configuration("foo");

            // Define a simple callback function
            Func<IEvent, bool> callback1 = (e) => true;
            Func<IEvent, bool> callback2 = (e) => false;
            Func<ISession, bool> sessionCallback = (s) => true;

            // We will use these lists to store the results from multiple threads
            List<bool> onErrorResults = new List<bool>();
            List<bool> onSendErrorResults = new List<bool>();
            List<bool> onSessionResults = new List<bool>();

            // Adding callbacks in multiple threads
            Thread addThread1 = new Thread(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    config.AddOnError(callback1);
                    config.AddOnSendError(callback2);
                    config.AddOnSession(sessionCallback);
                }
            });

            Thread addThread2 = new Thread(() =>
            {
                for (int i = 0; i < 50; i++)
                {
                    config.AddOnError(callback2);
                    config.AddOnSendError(callback1);
                    config.AddOnSession(sessionCallback);
                }
            });

            // Removing callbacks in multiple threads
            Thread removeThread1 = new Thread(() =>
            {
                for (int i = 0; i < 25; i++)
                {
                    config.RemoveOnError(callback1);
                    config.RemoveOnSendError(callback2);
                    config.RemoveOnSession(sessionCallback);
                }
            });

            Thread removeThread2 = new Thread(() =>
            {
                for (int i = 0; i < 25; i++)
                {
                    config.RemoveOnError(callback2);
                    config.RemoveOnSendError(callback1);
                    config.RemoveOnSession(sessionCallback);
                }
            });

            // Start all threads
            addThread1.Start();
            addThread2.Start();
            removeThread1.Start();
            removeThread2.Start();

            // Wait for all threads to complete
            addThread1.Join();
            addThread2.Join();
            removeThread1.Join();
            removeThread2.Join();

            // Verify the state of the callback lists
            // The exact number might vary depending on the execution order,
            // but there should be no exceptions thrown and the list should not be empty
            Assert.IsTrue(config.GetOnErrorCallbacks().Count > 0, "OnErrorCallbacks should have entries.");
            Assert.IsTrue(config.GetOnSendErrorCallbacks().Count > 0, "OnSendErrorCallbacks should have entries.");
            Assert.IsTrue(config.GetOnSessionCallbacks().Count > 0, "OnSessionCallbacks should have entries.");

            // Check if the remaining callbacks are as expected
            Assert.IsTrue(config.GetOnErrorCallbacks().Contains(callback1) || config.GetOnErrorCallbacks().Contains(callback2), "Callback1 or Callback2 should be in OnErrorCallbacks.");
            Assert.IsTrue(config.GetOnSendErrorCallbacks().Contains(callback1) || config.GetOnSendErrorCallbacks().Contains(callback2), "Callback1 or Callback2 should be in OnSendErrorCallbacks.");
            Assert.IsTrue(config.GetOnSessionCallbacks().Contains(sessionCallback), "SessionCallback should be in OnSessionCallbacks.");
        }
    }
}
