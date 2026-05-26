using NUnit.Framework;
using BugsnagUnity;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class LastRunInfoTests
    {
        [Test]
        public void DefaultValues_AreZeroAndFalse()
        {
            var info = new LastRunInfo();
            Assert.AreEqual(0, info.ConsecutiveLaunchCrashes);
            Assert.IsFalse(info.Crashed);
            Assert.IsFalse(info.CrashedDuringLaunch);
        }

        [Test]
        public void CanSet_ConsecutiveLaunchCrashes()
        {
            var info = new LastRunInfo();
            info.ConsecutiveLaunchCrashes = 3;
            Assert.AreEqual(3, info.ConsecutiveLaunchCrashes);
        }

        [Test]
        public void CanSet_Crashed()
        {
            var info = new LastRunInfo();
            info.Crashed = true;
            Assert.IsTrue(info.Crashed);
        }

        [Test]
        public void CanSet_CrashedDuringLaunch()
        {
            var info = new LastRunInfo();
            info.CrashedDuringLaunch = true;
            Assert.IsTrue(info.CrashedDuringLaunch);
        }

        [Test]
        public void AllFieldsIndependent()
        {
            var info = new LastRunInfo();
            info.Crashed = true;
            info.ConsecutiveLaunchCrashes = 5;

            // Setting Crashed should not affect CrashedDuringLaunch
            Assert.IsFalse(info.CrashedDuringLaunch);
        }
    }
}
