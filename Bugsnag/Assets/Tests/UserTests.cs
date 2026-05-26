using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var user = new User("id1", "email@test.com", "Alice");
            Assert.AreEqual("id1", user.Id);
            Assert.AreEqual("email@test.com", user.Email);
            Assert.AreEqual("Alice", user.Name);
        }

        [Test]
        public void InternalConstructor_ReturnsEmptyStrings()
        {
            var user = new User();
            Assert.AreEqual(string.Empty, user.Id);
            Assert.AreEqual(string.Empty, user.Email);
            Assert.AreEqual(string.Empty, user.Name);
        }

        [Test]
        public void Id_CanBeSetAfterConstruction()
        {
            var user = new User();
            user.Id = "newId";
            Assert.AreEqual("newId", user.Id);
        }

        [Test]
        public void Email_CanBeSetAfterConstruction()
        {
            var user = new User();
            user.Email = "newemail@test.com";
            Assert.AreEqual("newemail@test.com", user.Email);
        }

        [Test]
        public void Name_CanBeSetAfterConstruction()
        {
            var user = new User();
            user.Name = "Bob";
            Assert.AreEqual("Bob", user.Name);
        }

        [Test]
        public void Clone_ReturnsUserWithSameProperties()
        {
            var user = new User("id2", "clone@test.com", "Clone");
            var clone = user.Clone();
            Assert.AreEqual(user.Id, clone.Id);
            Assert.AreEqual(user.Email, clone.Email);
            Assert.AreEqual(user.Name, clone.Name);
        }

        [Test]
        public void NullId_ReturnsEmptyString()
        {
            var user = new User(null, null, null);
            Assert.AreEqual(string.Empty, user.Id);
            Assert.AreEqual(string.Empty, user.Email);
            Assert.AreEqual(string.Empty, user.Name);
        }
    }
}
