using NUnit.Framework;
using BugsnagUnity;
using System.Collections.Generic;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class TypeNameHelperTests
    {
        [Test]
        public void GetTypeDisplayName_NullObject_ReturnsNull()
        {
            var result = TypeNameHelper.GetTypeDisplayName((object)null);
            Assert.IsNull(result);
        }

        [Test]
        public void GetTypeDisplayName_IntType_ReturnsBuiltInName()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(int), fullName: false);
            Assert.AreEqual("int", result);
        }

        [Test]
        public void GetTypeDisplayName_StringType_ReturnsBuiltInName()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(string), fullName: false);
            Assert.AreEqual("string", result);
        }

        [Test]
        public void GetTypeDisplayName_BoolType_ReturnsBuiltInName()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(bool), fullName: false);
            Assert.AreEqual("bool", result);
        }

        [Test]
        public void GetTypeDisplayName_Class_FullName_ReturnsFullyQualifiedName()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(TypeNameHelperTests), fullName: true);
            Assert.AreEqual("BugsnagUnityTests.TypeNameHelperTests", result);
        }

        [Test]
        public void GetTypeDisplayName_Class_ShortName_ReturnsUnqualifiedName()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(TypeNameHelperTests), fullName: false);
            Assert.AreEqual("TypeNameHelperTests", result);
        }

        [Test]
        public void GetTypeDisplayName_GenericList_ContainsListWithAngleBrackets()
        {
            // GetGenericTypeDefinition().GetGenericArguments() returns open type params (T),
            // not the concrete type (int), so output is "List<>" not "List<int>".
            var result = TypeNameHelper.GetTypeDisplayName(typeof(List<int>), fullName: false);
            Assert.IsTrue(result.Contains("List"), result);
            Assert.IsTrue(result.Contains("<"), result);
            Assert.IsTrue(result.Contains(">"), result);
        }

        [Test]
        public void GetTypeDisplayName_IntArray_ContainsBrackets()
        {
            var result = TypeNameHelper.GetTypeDisplayName(typeof(int[]), fullName: false);
            Assert.IsTrue(result.Contains("[]"), result);
            Assert.IsTrue(result.Contains("int"), result);
        }

        [Test]
        public void GetTypeDisplayName_ObjectInstance_MatchesTypeOverload()
        {
            var obj = new List<string>();
            var fromInstance = TypeNameHelper.GetTypeDisplayName(obj, fullName: false);
            var fromType = TypeNameHelper.GetTypeDisplayName(typeof(List<string>), fullName: false);
            Assert.AreEqual(fromType, fromInstance);
        }
    }
}
