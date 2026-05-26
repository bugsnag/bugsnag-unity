using NUnit.Framework;
using BugsnagUnity;
using BugsnagUnity.Payload;
using System;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class DeviceTests
    {
        private static Device MakeDevice(Dictionary<string, object> data = null)
            => new Device(data ?? new Dictionary<string, object>());

        // ---- Id ----

        [Test]
        public void Id_SetAndGet()
        {
            var d = MakeDevice();
            d.Id = "device-123";
            Assert.AreEqual("device-123", d.Id);
        }

        [Test]
        public void Id_Default_IsNull()
        {
            var d = MakeDevice();
            Assert.IsNull(d.Id);
        }

        // ---- Jailbroken ----

        [Test]
        public void Jailbroken_SetTrue_ReturnedTrue()
        {
            var d = MakeDevice();
            d.Jailbroken = true;
            Assert.AreEqual(true, d.Jailbroken);
        }

        [Test]
        public void Jailbroken_Default_IsNull()
        {
            var d = MakeDevice();
            Assert.IsNull(d.Jailbroken);
        }

        // ---- Locale ----

        [Test]
        public void Locale_SetAndGet()
        {
            var d = MakeDevice();
            d.Locale = "en-GB";
            Assert.AreEqual("en-GB", d.Locale);
        }

        // ---- Manufacturer ----

        [Test]
        public void Manufacturer_SetAndGet()
        {
            var d = MakeDevice();
            d.Manufacturer = "Apple";
            Assert.AreEqual("Apple", d.Manufacturer);
        }

        // ---- Model ----

        [Test]
        public void Model_SetAndGet()
        {
            var d = MakeDevice();
            d.Model = "iPhone 15";
            Assert.AreEqual("iPhone 15", d.Model);
        }

        // ---- ModelNumber ----

        [Test]
        public void ModelNumber_SetAndGet()
        {
            var d = MakeDevice();
            d.ModelNumber = "A2846";
            Assert.AreEqual("A2846", d.ModelNumber);
        }

        // ---- OsName ----

        [Test]
        public void OsName_SetAndGet()
        {
            var d = MakeDevice();
            d.OsName = "iOS";
            Assert.AreEqual("iOS", d.OsName);
        }

        // ---- OsVersion ----

        [Test]
        public void OsVersion_SetAndGet()
        {
            var d = MakeDevice();
            d.OsVersion = "17.0";
            Assert.AreEqual("17.0", d.OsVersion);
        }

        // ---- TotalMemory ----

        [Test]
        public void TotalMemory_SetAndGet()
        {
            var d = MakeDevice();
            d.TotalMemory = 8192L;
            Assert.AreEqual(8192L, d.TotalMemory);
        }

        // ---- RuntimeVersions ----

        [Test]
        public void RuntimeVersions_SetAndGet()
        {
            var d = MakeDevice();
            var rv = new Dictionary<string, object> { { "unity", "2021.3" } };
            d.RuntimeVersions = rv;
            Assert.IsNotNull(d.RuntimeVersions);
            Assert.AreEqual("2021.3", d.RuntimeVersions["unity"]);
        }

        // ---- CpuAbi ----

        [Test]
        public void CpuAbi_SetAndGet()
        {
            var d = MakeDevice();
            d.CpuAbi = new[] { "arm64-v8a", "x86" };
            CollectionAssert.AreEqual(new[] { "arm64-v8a", "x86" }, d.CpuAbi);
        }

        // ---- BrowserName / BrowserVersion ----

        [Test]
        public void BrowserName_SetAndGet()
        {
            var d = MakeDevice();
            d.BrowserName = "Chrome";
            Assert.AreEqual("Chrome", d.BrowserName);
        }

        [Test]
        public void BrowserVersion_SetAndGet()
        {
            var d = MakeDevice();
            d.BrowserVersion = "120.0";
            Assert.AreEqual("120.0", d.BrowserVersion);
        }

        // ---- UserAgent ----

        [Test]
        public void UserAgent_SetAndGet()
        {
            var d = MakeDevice();
            d.UserAgent = "Mozilla/5.0";
            Assert.AreEqual("Mozilla/5.0", d.UserAgent);
        }

        // ---- CachedData constructor populates fields ----

        [Test]
        public void CachedData_Constructor_PopulatesId()
        {
            var data = new Dictionary<string, object> { { "id", "cached-id" } };
            var d = MakeDevice(data);
            Assert.AreEqual("cached-id", d.Id);
        }
    }

    [TestFixture]
    public class DeviceWithStateTests
    {
        private static DeviceWithState Make(Dictionary<string, object> data = null)
            => new DeviceWithState(data ?? new Dictionary<string, object>());

        // ---- FreeDisk ----

        [Test]
        public void FreeDisk_SetAndGet()
        {
            var d = Make();
            d.FreeDisk = 512000L;
            Assert.AreEqual(512000L, d.FreeDisk);
        }

        [Test]
        public void FreeDisk_Default_IsNull()
        {
            var d = Make();
            Assert.IsNull(d.FreeDisk);
        }

        // ---- FreeMemory ----

        [Test]
        public void FreeMemory_SetAndGet()
        {
            var d = Make();
            d.FreeMemory = 204800L;
            Assert.AreEqual(204800L, d.FreeMemory);
        }

        // ---- Orientation ----

        [Test]
        public void Orientation_SetAndGet()
        {
            var d = Make();
            d.Orientation = "Portrait";
            Assert.AreEqual("Portrait", d.Orientation);
        }

        // ---- Time ----

        [Test]
        public void Time_SetAndGet()
        {
            var d = Make();
            var now = DateTimeOffset.Now;
            d.Time = now;
            Assert.AreEqual(now, d.Time);
        }

        [Test]
        public void Time_Default_IsNull()
        {
            var d = Make();
            Assert.IsNull(d.Time);
        }

        // ---- Inherits Device properties ----

        [Test]
        public void InheritsDevice_Model_SetAndGet()
        {
            var d = Make();
            d.Model = "Pixel 7";
            Assert.AreEqual("Pixel 7", d.Model);
        }

        [Test]
        public void CachedData_Constructor_PopulatesOrientation()
        {
            var data = new Dictionary<string, object> { { "orientation", "Landscape" } };
            var d = Make(data);
            Assert.AreEqual("Landscape", d.Orientation);
        }
    }
}
