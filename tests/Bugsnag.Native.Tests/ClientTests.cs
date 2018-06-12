using NUnit.Framework;
using System.Collections.Generic;

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
    public void RegisterWithApiKeyTest() => Client.Register("test", new Dictionary<string, string>());

    [Test]
    public void RegisterWithApiKeyAndAutoTrackSessionsTest() => Client.Register("test", false, new Dictionary<string, string>());

    [Test]
    public void SetNotifyUrlTest() => Client.SetNotifyUrl("test");

    [Test]
    public void SetAutoNotifyTest() => Client.SetAutoNotify(true);

    [Test]
    public void SetContext() => Client.SetContext("context");

    [Test]
    public void SetReleaseStage() => Client.SetReleaseStage("production");

    [Test]
    public void SetNotifyReleaseStages() => Client.SetNotifyReleaseStages(new string[] { "production" });

    [Test]
    public void AddToTab() => Client.AddToTab("tabName", "attributeName", "attributeValue");

    [Test]
    public void ClearTab() => Client.ClearTab("tabName");

    [Test]
    public void LeaveBreadcrumb() => Client.LeaveBreadcrumb("breadcrumb");

    [Test]
    public void SetBreadcrumbCapacity() => Client.SetBreadcrumbCapacity(0);

    [Test]
    public void SetAppVersion() => Client.SetAppVersion("1.0.0");

    [Test]
    public void SetUser() => Client.SetUser("1", "bugsnag", "support@bugsnag.com");
  }
}
