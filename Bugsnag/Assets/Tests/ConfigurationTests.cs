using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using UnityEngine.TestTools;
using BugsnagUnity;
using BugsnagUnity.Payload;
using UnityEngine;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class ConfigurationTests
    {
        private const string DEFAULT_NOTIFY = "https://notify.bugsnag.com/";
        private const string DEFAULT_SESSION = "https://sessions.bugsnag.com/";
        private const string SECONDARY_NOTIFY = "https://notify.bugsnag.smartbear.com/";
        private const string SECONDARY_SESSION = "https://sessions.bugsnag.smartbear.com/";
        private const string CUSTOM_NOTIFY = "https://www.customnotify.com/";
        private const string CUSTOM_SESSION = "https://www.customsession.com/";
        [Test]
        public void DefaultConfigurationValues()
        {
            var config = new Configuration("foo");
            config.Endpoints.Configure("foo");
            Assert.IsTrue(config.ReportExceptionLogsAsHandled);
            Assert.IsTrue(config.AutoDetectErrors);
            Assert.IsTrue(config.AutoTrackSessions);
            Assert.AreEqual("production", config.ReleaseStage);
            Assert.AreEqual(DEFAULT_NOTIFY, config.Endpoints.NotifyEndpoint.ToString());
            Assert.AreEqual(DEFAULT_SESSION, config.Endpoints.SessionEndpoint.ToString());
            Assert.AreEqual("foo", config.ApiKey);
        }

        [Test]
        public void MaxBreadcrumbsLimit()
        {
            var config = new Configuration("foo");
            LogAssert.Expect(LogType.Error, "Invalid configuration value detected. Option maxBreadcrumbs should be an integer between 0-500. Supplied value is 501");
            config.MaximumBreadcrumbs = 501;
            Assert.AreEqual(100, config.MaximumBreadcrumbs);
            LogAssert.Expect(LogType.Error, "Invalid configuration value detected. Option maxBreadcrumbs should be an integer between 0-500. Supplied value is -1");
            config.MaximumBreadcrumbs = -1;
            Assert.AreEqual(100, config.MaximumBreadcrumbs);
            config.MaximumBreadcrumbs = 20;
            Assert.AreEqual(20, config.MaximumBreadcrumbs);
        }

        [Test]
        public void CloneTest()
        {
            var original = new Configuration("foo");

            original.MaximumBreadcrumbs = 1;
            original.ReleaseStage = "1";
            original.SetUser("1", "1", "1");

            var clone = original.Clone();

            // int check
            Assert.AreEqual(original.MaximumBreadcrumbs, clone.MaximumBreadcrumbs);
            clone.MaximumBreadcrumbs = 2;
            Assert.AreEqual(1, original.MaximumBreadcrumbs);
            Assert.AreEqual(2, clone.MaximumBreadcrumbs);

            // string check
            clone.ReleaseStage = "2";
            Assert.AreNotEqual(original.ReleaseStage, clone.ReleaseStage);

            // user check
            clone.SetUser("2", "2", "2");
            Assert.AreEqual("1", original.GetUser().Name);
            Assert.AreEqual("2", clone.GetUser().Name);
        }




        //-----------------------------------------------------------------
        // DEFAULTS (no custom endpoints, api‑key that does NOT trigger the
        //           alternate endpoints)
        //-----------------------------------------------------------------
        [Test]
        public void Configure_SetsDefaultEndpoints_WhenNoCustomisation()
        {
            var cfg = new EndpointConfiguration();
            cfg.Configure("foo-bar");

            Assert.That(cfg.IsConfigured, Is.True);
            Assert.That(cfg.NotifyEndpoint.ToString(), Is.EqualTo(DEFAULT_NOTIFY));
            Assert.That(cfg.SessionEndpoint.ToString(), Is.EqualTo(DEFAULT_SESSION));
        }

        //-----------------------------------------------------------------
        // ALTERNATE (api‑key with leading "00000" should map to InsightHub)
        //-----------------------------------------------------------------
        [Test]
        public void Configure_SetsAlternateEndpoints_WhenApiKeyStartsWith00000()
        {
            var cfg = new EndpointConfiguration();
            cfg.Configure("00000abcdef");

            Assert.That(cfg.IsConfigured, Is.True);
            Assert.That(cfg.NotifyEndpoint.ToString(), Is.EqualTo(SECONDARY_NOTIFY));
            Assert.That(cfg.SessionEndpoint.ToString(), Is.EqualTo(SECONDARY_SESSION));
        }

        //-----------------------------------------------------------------
        // CUSTOM (both endpoints supplied explicitly)
        //-----------------------------------------------------------------
        [Test]
        public void Configure_UsesCustomEndpoints_WhenBothProvided()
        {
            var cfg = new EndpointConfiguration(CUSTOM_NOTIFY, CUSTOM_SESSION);
            cfg.Configure("foo-bar");

            Assert.That(cfg.IsConfigured, Is.True);
            Assert.That(cfg.NotifyEndpoint.ToString(), Is.EqualTo(CUSTOM_NOTIFY));
            Assert.That(cfg.SessionEndpoint.ToString(), Is.EqualTo(CUSTOM_SESSION));
        }

        //-----------------------------------------------------------------
        // INVALID: Notify only
        //-----------------------------------------------------------------
        [Test]
        public void Configure_Fails_WhenOnlyNotifyCustomised()
        {
            var cfg = new EndpointConfiguration(CUSTOM_NOTIFY, string.Empty);
            cfg.Configure("foo-bar");

            Assert.That(cfg.IsConfigured, Is.False, "Partial customisation should leave config in an unconfigured state");
            Assert.That(cfg.NotifyEndpoint, Is.Null);
            Assert.That(cfg.SessionEndpoint, Is.Null);
        }

        //-----------------------------------------------------------------
        // INVALID: Session only
        //-----------------------------------------------------------------
        [Test]
        public void Configure_Fails_WhenOnlySessionCustomised()
        {
            var cfg = new EndpointConfiguration(string.Empty, CUSTOM_SESSION);
            cfg.Configure("foo-bar");

            Assert.That(cfg.IsConfigured, Is.False);
            Assert.That(cfg.NotifyEndpoint, Is.Null);
            Assert.That(cfg.SessionEndpoint, Is.Null);
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

            // Define simple callback functions
            Func<IEvent, bool> callback1 = (e) => true;
            Func<IEvent, bool> callback2 = (e) => false;
            Func<ISession, bool> sessionCallback = (s) => true;

            // Lists to store results from multiple threads
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
            Assert.IsTrue(config.GetOnErrorCallbacks().Count > 0, "OnErrorCallbacks should have entries.");
            Assert.IsTrue(config.GetOnSendErrorCallbacks().Count > 0, "OnSendErrorCallbacks should have entries.");
            Assert.IsTrue(config.GetOnSessionCallbacks().Count > 0, "OnSessionCallbacks should have entries.");

            // Check if the remaining callbacks are as expected
            Assert.IsTrue(config.GetOnErrorCallbacks().Contains(callback1) || config.GetOnErrorCallbacks().Contains(callback2), "Callback1 or Callback2 should be in OnErrorCallbacks.");
            Assert.IsTrue(config.GetOnSendErrorCallbacks().Contains(callback1) || config.GetOnSendErrorCallbacks().Contains(callback2), "Callback1 or Callback2 should be in OnSendErrorCallbacks.");
            Assert.IsTrue(config.GetOnSessionCallbacks().Contains(sessionCallback), "SessionCallback should be in OnSessionCallbacks.");
        }

        [Test]
        public void AppHangThreshold_BelowMinimum_IsIgnored()
        {
            var config = new Configuration("foo");
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*AppHangThresholdMillis.*"));
            config.AppHangThresholdMillis = 100;
            Assert.AreEqual(0UL, config.AppHangThresholdMillis);
        }

        [Test]
        public void AppHangThreshold_AtMinimum_IsAccepted()
        {
            var config = new Configuration("foo");
            config.AppHangThresholdMillis = 250;
            Assert.AreEqual(250UL, config.AppHangThresholdMillis);
        }

        [Test]
        public void AppHangThreshold_AboveMinimum_IsAccepted()
        {
            var config = new Configuration("foo");
            config.AppHangThresholdMillis = 5000;
            Assert.AreEqual(5000UL, config.AppHangThresholdMillis);
        }

        [Test]
        public void ShouldLeaveLogBreadcrumb_DefaultBreadcrumbLogLevel_ExceptionIsEnabled()
        {
            var config = new Configuration("foo");
            // Default BreadcrumbLogLevel is LogType.Log, so Exception (higher severity) should be included
            Assert.IsTrue(config.ShouldLeaveLogBreadcrumb(LogType.Exception));
        }

        [Test]
        public void ShouldLeaveLogBreadcrumb_LogBreadcrumbTypeDisabled_ReturnsFalse()
        {
            var config = new Configuration("foo");
            config.EnabledBreadcrumbTypes = new BreadcrumbType[] { BreadcrumbType.Error };
            Assert.IsFalse(config.ShouldLeaveLogBreadcrumb(LogType.Exception));
        }

        [Test]
        public void IsBreadcrumbTypeEnabled_NullEnabledTypes_AllTypesEnabled()
        {
            var config = new Configuration("foo");
            config.EnabledBreadcrumbTypes = null;
            Assert.IsTrue(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Log));
            Assert.IsTrue(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Error));
            Assert.IsTrue(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Navigation));
        }

        [Test]
        public void IsBreadcrumbTypeEnabled_LimitedTypes_OnlySpecifiedEnabled()
        {
            var config = new Configuration("foo");
            config.EnabledBreadcrumbTypes = new BreadcrumbType[] { BreadcrumbType.Error };
            Assert.IsTrue(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Error));
            Assert.IsFalse(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Log));
            Assert.IsFalse(config.IsBreadcrumbTypeEnabled(BreadcrumbType.Navigation));
        }

        [Test]
        public void SetUser_AndGetUser_RoundTrip()
        {
            var config = new Configuration("foo");
            config.SetUser("id-1", "user@example.com", "Alice");
            var user = config.GetUser();
            Assert.AreEqual("id-1", user.Id);
            Assert.AreEqual("user@example.com", user.Email);
            Assert.AreEqual("Alice", user.Name);
        }

        [Test]
        public void GetUser_BeforeSetUser_ReturnsNull()
        {
            var config = new Configuration("foo");
            Assert.IsNull(config.GetUser());
        }

        [Test]
        public void AddFeatureFlag_CanBeRetrieved()
        {
            var config = new Configuration("foo");
            config.AddFeatureFlag("flag-a", "variant-1");
            Assert.IsTrue(config.FeatureFlags.Contains("flag-a"));
            Assert.AreEqual("variant-1", config.FeatureFlags["flag-a"]);
        }

        [Test]
        public void AddFeatureFlag_WithoutVariant_StoredAsNull()
        {
            var config = new Configuration("foo");
            config.AddFeatureFlag("no-variant");
            Assert.IsTrue(config.FeatureFlags.Contains("no-variant"));
            Assert.IsNull(config.FeatureFlags["no-variant"]);
        }

        [Test]
        public void ClearFeatureFlag_RemovesSpecificFlag()
        {
            var config = new Configuration("foo");
            config.AddFeatureFlag("flag-a", "v1");
            config.AddFeatureFlag("flag-b", "v2");
            config.ClearFeatureFlag("flag-a");
            Assert.IsFalse(config.FeatureFlags.Contains("flag-a"));
            Assert.IsTrue(config.FeatureFlags.Contains("flag-b"));
        }

        [Test]
        public void ClearFeatureFlags_RemovesAllFlags()
        {
            var config = new Configuration("foo");
            config.AddFeatureFlag("flag-a", "v1");
            config.AddFeatureFlag("flag-b", "v2");
            config.ClearFeatureFlags();
            Assert.AreEqual(0, config.FeatureFlags.Count);
        }

        [Test]
        public void AddFeatureFlags_BulkAddition()
        {
            var config = new Configuration("foo");
            config.AddFeatureFlags(new[] {
                new FeatureFlag("bulk-a", "v1"),
                new FeatureFlag("bulk-b", "v2"),
            });
            Assert.AreEqual(2, config.FeatureFlags.Count);
            Assert.AreEqual("v1", config.FeatureFlags["bulk-a"]);
            Assert.AreEqual("v2", config.FeatureFlags["bulk-b"]);
        }

        [Test]
        public void AddMetadata_CanBeRetrieved()
        {
            var config = new Configuration("foo");
            config.AddMetadata("section", "key", "value");
            Assert.AreEqual("value", config.GetMetadata("section", "key"));
        }

        [Test]
        public void AddMetadata_IDictionary_AllKeysCanBeRetrieved()
        {
            var config = new Configuration("foo");
            var dict = new Dictionary<string, object>
            {
                { "key1", "val1" },
                { "key2", 42 }
            };
            config.AddMetadata("section", (IDictionary<string, object>)dict);
            Assert.AreEqual("val1", config.GetMetadata("section", "key1"));
            Assert.AreEqual(42, config.GetMetadata("section", "key2"));
        }

        [Test]
        public void GetAssemblyName_ReturnsNonNull()
        {
            var name = Configuration.GetAssemblyName();
            Assert.IsNotNull(name);
            Assert.IsNotNull(name.Name);
        }

        [Test]
        public void ClearMetadata_Section_RemovesAllKeysInSection()
        {
            var config = new Configuration("foo");
            config.AddMetadata("section", "key1", "val1");
            config.AddMetadata("section", "key2", "val2");
            config.ClearMetadata("section");
            Assert.IsNull(config.GetMetadata("section"));
        }

        [Test]
        public void ClearMetadata_SectionAndKey_RemovesSingleKey()
        {
            var config = new Configuration("foo");
            config.AddMetadata("section", "key1", "val1");
            config.AddMetadata("section", "key2", "val2");
            config.ClearMetadata("section", "key1");
            var section = config.GetMetadata("section") as System.Collections.Generic.IDictionary<string, object>;
            Assert.IsNotNull(section);
            Assert.IsFalse(section.ContainsKey("key1"));
            Assert.AreEqual("val2", section["key2"]);
        }

        [Test]
        public void Configure_EmptyApiKey_DoesNotConfigure()
        {
            var cfg = new EndpointConfiguration();
            cfg.Configure(string.Empty);
            Assert.IsFalse(cfg.IsConfigured);
        }

        [Test]
        public void Configure_IsIdempotent_SecondCallHasNoEffect()
        {
            var cfg = new EndpointConfiguration();
            cfg.Configure("foo");
            Assert.IsTrue(cfg.IsConfigured);
            // Calling again with a 00000 key should not change the already-configured endpoints
            cfg.Configure("00000abcde");
            Assert.AreEqual(DEFAULT_NOTIFY, cfg.NotifyEndpoint.ToString());
        }

        [Test]
        public void Configure_InvalidNotifyUri_DoesNotConfigure()
        {
            var cfg = new EndpointConfiguration("not a valid uri :// !!!", "https://sessions.example.com");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*NotifyEndpoint.*"));
            cfg.Configure("foo");
            Assert.IsFalse(cfg.IsConfigured);
        }

        [Test]
        public void Configure_InvalidSessionUri_DoesNotConfigure()
        {
            var cfg = new EndpointConfiguration("https://notify.example.com", "not a valid uri :// !!!");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(".*SessionEndpoint.*"));
            cfg.Configure("foo");
            Assert.IsFalse(cfg.IsConfigured);
        }
    }

    [TestFixture]
    public class EndpointConfigurationTests
    {
        [Test]
        public void Configure_DefaultApiKey_SetsNotifyEndpoint()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("my-api-key-1234");
            Assert.AreEqual("https://notify.bugsnag.com", ec.NotifyEndpoint.ToString().TrimEnd('/'));
        }

        [Test]
        public void Configure_DefaultApiKey_SetsSessionEndpoint()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("my-api-key-1234");
            Assert.AreEqual("https://sessions.bugsnag.com", ec.SessionEndpoint.ToString().TrimEnd('/'));
        }

        [Test]
        public void Configure_SetsIsConfiguredTrue()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("regular-key");
            Assert.IsTrue(ec.IsConfigured);
        }

        [Test]
        public void Configure_SecondaryApiKey_SetsSecondaryNotifyEndpoint()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("00000abc-key");
            StringAssert.Contains("smartbear.com", ec.NotifyEndpoint.ToString());
        }

        [Test]
        public void Configure_SecondaryApiKey_SetsSecondarySessionEndpoint()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("00000abc-key");
            StringAssert.Contains("smartbear.com", ec.SessionEndpoint.ToString());
        }

        [Test]
        public void Configure_CustomEndpoints_UsesCustomNotify()
        {
            var ec = new EndpointConfiguration("https://custom.notify.example.com", "https://custom.sessions.example.com");
            ec.Configure("any-key");
            Assert.AreEqual("https://custom.notify.example.com", ec.NotifyEndpoint.ToString().TrimEnd('/'));
        }

        [Test]
        public void Configure_CustomEndpoints_UsesCustomSession()
        {
            var ec = new EndpointConfiguration("https://custom.notify.example.com", "https://custom.sessions.example.com");
            ec.Configure("any-key");
            Assert.AreEqual("https://custom.sessions.example.com", ec.SessionEndpoint.ToString().TrimEnd('/'));
        }

        [Test]
        public void Configure_OnlyNotifyCustomised_IsNotConfigured()
        {
            var ec = new EndpointConfiguration("https://custom.notify.example.com", "");
            ec.Configure("any-key");
            Assert.IsFalse(ec.IsConfigured);
        }

        [Test]
        public void Configure_OnlySessionCustomised_IsNotConfigured()
        {
            var ec = new EndpointConfiguration("", "https://custom.sessions.example.com");
            ec.Configure("any-key");
            Assert.IsFalse(ec.IsConfigured);
        }

        [Test]
        public void Configure_CalledTwice_DoesNotReconfigure()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("first-key");
            var original = ec.NotifyEndpoint;
            ec.Configure("00000different-key");
            Assert.AreEqual(original, ec.NotifyEndpoint);
        }

        [Test]
        public void Configure_EmptyApiKey_DoesNotConfigure()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("");
            Assert.IsFalse(ec.IsConfigured);
        }

        [Test]
        public void Clone_ReturnsNewInstance()
        {
            var ec = new EndpointConfiguration();
            ec.Configure("key");
            var clone = ec.Clone();
            Assert.AreNotSame(ec, clone);
            Assert.AreEqual(ec.NotifyEndpoint, clone.NotifyEndpoint);
        }
    }
}