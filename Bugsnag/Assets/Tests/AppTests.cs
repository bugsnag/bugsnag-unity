using NUnit.Framework;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class AppTests
    {
        private App EmptyApp() => new App(new Dictionary<string, object>());

        [Test]
        public void App_Id_SetAndGet()
        {
            var app = EmptyApp();
            app.Id = "com.example.app";
            Assert.AreEqual("com.example.app", app.Id);
        }

        [Test]
        public void App_Version_SetAndGet()
        {
            var app = EmptyApp();
            app.Version = "1.2.3";
            Assert.AreEqual("1.2.3", app.Version);
        }

        [Test]
        public void App_ReleaseStage_SetAndGet()
        {
            var app = EmptyApp();
            app.ReleaseStage = "production";
            Assert.AreEqual("production", app.ReleaseStage);
        }

        [Test]
        public void App_Type_SetAndGet()
        {
            var app = EmptyApp();
            app.Type = "iOS";
            Assert.AreEqual("iOS", app.Type);
        }

        [Test]
        public void App_BundleVersion_SetAndGet()
        {
            var app = EmptyApp();
            app.BundleVersion = "42";
            Assert.AreEqual("42", app.BundleVersion);
        }

        [Test]
        public void App_BuildUuid_SetAndGet()
        {
            var app = EmptyApp();
            app.BuildUuid = "build-uuid-123";
            Assert.AreEqual("build-uuid-123", app.BuildUuid);
        }

        [Test]
        public void App_VersionCode_SetAndGet()
        {
            var app = EmptyApp();
            app.VersionCode = 99;
            Assert.AreEqual(99, app.VersionCode);
        }

        [Test]
        public void App_DefaultValues_AreNull()
        {
            var app = EmptyApp();
            Assert.IsNull(app.Id);
            Assert.IsNull(app.Version);
            Assert.IsNull(app.ReleaseStage);
            Assert.IsNull(app.Type);
        }

        [Test]
        public void App_CachedData_Constructor_RestoresValues()
        {
            var data = new Dictionary<string, object>
            {
                { "id", "com.test" },
                { "version", "2.0" }
            };
            var app = new App(data);
            Assert.AreEqual("com.test", app.Id);
            Assert.AreEqual("2.0", app.Version);
        }
    }

    [TestFixture]
    public class AppWithStateTests
    {
        private AppWithState EmptyAppWithState() =>
            new AppWithState(new Dictionary<string, object>());

        [Test]
        public void AppWithState_Duration_SetAndGet()
        {
            var app = EmptyAppWithState();
            app.Duration = TimeSpan.FromSeconds(30);
            Assert.IsNotNull(app.Duration);
            Assert.AreEqual(30, app.Duration.Value.TotalSeconds, 0.01);
        }

        [Test]
        public void AppWithState_DurationInForeground_SetAndGet()
        {
            var app = EmptyAppWithState();
            app.DurationInForeground = TimeSpan.FromSeconds(10);
            Assert.AreEqual(10, app.DurationInForeground.Value.TotalSeconds, 0.01);
        }

        [Test]
        public void AppWithState_InForeground_SetAndGet()
        {
            var app = EmptyAppWithState();
            app.InForeground = true;
            Assert.IsTrue(app.InForeground);
        }

        [Test]
        public void AppWithState_IsLaunching_SetAndGet()
        {
            var app = EmptyAppWithState();
            app.IsLaunching = true;
            Assert.IsTrue(app.IsLaunching);
        }

        [Test]
        public void AppWithState_DefaultDuration_IsNull()
        {
            var app = EmptyAppWithState();
            Assert.IsNull(app.Duration);
        }

        [Test]
        public void AppWithState_InheritsAppProperties()
        {
            var app = EmptyAppWithState();
            app.Version = "3.0";
            app.ReleaseStage = "staging";
            Assert.AreEqual("3.0", app.Version);
            Assert.AreEqual("staging", app.ReleaseStage);
        }
    }
}
