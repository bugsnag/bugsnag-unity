using NUnit.Framework;
using BugsnagUnity;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class LoadedImageTests
    {
        [Test]
        public void Constructor_SetsAllFields()
        {
            var img = new LoadedImage(0x1000UL, 512UL, "libMyLib.dylib", "uuid-123", true);
            Assert.AreEqual(0x1000UL, img.LoadAddress);
            Assert.AreEqual(512UL, img.Size);
            Assert.AreEqual("libMyLib.dylib", img.FileName);
            Assert.AreEqual("uuid-123", img.Uuid);
            Assert.IsTrue(img.IsMainImage);
        }

        [Test]
        public void IsMainImage_False_WhenPassedFalse()
        {
            var img = new LoadedImage(0x0UL, 0UL, "lib.dylib", "uuid-456", false);
            Assert.IsFalse(img.IsMainImage);
        }

        [Test]
        public void ZeroLoadAddress_IsStored()
        {
            var img = new LoadedImage(0UL, 1024UL, "test.dylib", "uuid-789", false);
            Assert.AreEqual(0UL, img.LoadAddress);
        }

        [Test]
        public void LargeLoadAddress_IsStored()
        {
            ulong addr = ulong.MaxValue;
            var img = new LoadedImage(addr, 0UL, "big.dylib", "uuid-000", false);
            Assert.AreEqual(addr, img.LoadAddress);
        }
    }
}
