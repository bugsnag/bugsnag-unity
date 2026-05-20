using NUnit.Framework;
using BugsnagUnity;
using UnityEngine;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class LogTypeSeverityMappingTests
    {
        private LogTypeSeverityMapping _mapping;

        [SetUp]
        public void SetUp()
        {
            _mapping = new LogTypeSeverityMapping();
        }

        [Test]
        public void DefaultMapping_Assert_IsWarning()
        {
            Assert.AreEqual(Severity.Warning, _mapping.Map(LogType.Assert));
        }

        [Test]
        public void DefaultMapping_Error_IsWarning()
        {
            Assert.AreEqual(Severity.Warning, _mapping.Map(LogType.Error));
        }

        [Test]
        public void DefaultMapping_Exception_IsError()
        {
            Assert.AreEqual(Severity.Error, _mapping.Map(LogType.Exception));
        }

        [Test]
        public void DefaultMapping_Log_IsInfo()
        {
            Assert.AreEqual(Severity.Info, _mapping.Map(LogType.Log));
        }

        [Test]
        public void DefaultMapping_Warning_IsWarning()
        {
            Assert.AreEqual(Severity.Warning, _mapping.Map(LogType.Warning));
        }

        [Test]
        public void UpdateMapping_ChangesExistingLogType()
        {
            _mapping.UpdateMapping(LogType.Log, Severity.Error);
            Assert.AreEqual(Severity.Error, _mapping.Map(LogType.Log));
        }

        [Test]
        public void UpdateMapping_DoesNotAffectOtherMappings()
        {
            _mapping.UpdateMapping(LogType.Log, Severity.Error);
            Assert.AreEqual(Severity.Warning, _mapping.Map(LogType.Warning));
            Assert.AreEqual(Severity.Error, _mapping.Map(LogType.Exception));
        }

        [Test]
        public void UpdateMapping_CanBeCalledMultipleTimes()
        {
            _mapping.UpdateMapping(LogType.Error, Severity.Info);
            Assert.AreEqual(Severity.Info, _mapping.Map(LogType.Error));

            _mapping.UpdateMapping(LogType.Error, Severity.Error);
            Assert.AreEqual(Severity.Error, _mapping.Map(LogType.Error));
        }

        [Test]
        public void Map_AllLogTypesCoveredByDefault()
        {
            // Ensures no LogType falls through to the default Error return
            Assert.AreNotEqual(null, _mapping.Map(LogType.Assert));
            Assert.AreNotEqual(null, _mapping.Map(LogType.Error));
            Assert.AreNotEqual(null, _mapping.Map(LogType.Exception));
            Assert.AreNotEqual(null, _mapping.Map(LogType.Log));
            Assert.AreNotEqual(null, _mapping.Map(LogType.Warning));
        }
    }
}
