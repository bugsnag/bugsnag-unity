using NUnit.Framework;

namespace Bugsnag.Native.Tests
{
  /// <summary>
  /// These tests are about ensuring that the API for the native wrapper dlls
  /// is consistent across the platforms. We are not testing functionality here
  /// so we only need to call each of the methods that we want to test
  /// </summary>
  [TestFixture]
  public class ClientTests
  {
    [Test]
    public void RegisterWithApiKeyTest() => Client.Register("test");

    [Test]
    public void RegisterWithApiKeyAndAutoTrackSessionsTest() => Client.Register("test", false);

    [Test]
    public void SetNotifyUrlTest() => Client.SetNotifyUrl("test");
  }
}
