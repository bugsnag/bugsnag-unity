using System.Collections.Generic;
using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnity.Payload.Tests
{
  class MetadataTests
  {
    [Test]
    public void AddMetadataValues()
    {
      var metadata = new Metadata();
      metadata.Add("foo", "bar");
      var custom = (Dictionary<string, string>)metadata["custom"];
      Assert.AreEqual("bar", custom["foo"]);
    }

    [Test]
    public void OverwriteMetadataValues()
    {
      var metadata = new Metadata();
      metadata.Add("foo", "bar");
      metadata.Add("foo", "baz");
      var custom = (Dictionary<string, string>)metadata["custom"];
      Assert.AreEqual(1, custom.Count);
      Assert.AreEqual("baz", custom["foo"]);
    }

    [Test]
    public void AddNonDictValueToCustomTab()
    {
      var metadata = new Metadata();
      metadata.Add("foo", "31");
      metadata.Add("bar", "hello");
      var custom = (Dictionary<string, string>)metadata["custom"];
      Assert.AreEqual("31", custom["foo"]);
      Assert.AreEqual("hello", custom["bar"]);
    }

    [Test]
    public void AddNonStringValueToCustomTab()
    {
      var metadata = new Metadata();
      metadata.Add("foo", 31);
      var custom = (Dictionary<string, string>)metadata["custom"];
      Assert.AreEqual("31", custom["foo"]);
    }

    [Test]
    public void MergeMetadataValues()
    {
      var metadata = new Metadata();
      metadata.Add("foo", new Dictionary<string, string> {{ "bar", "baz" }});
      metadata.Add("foo", new Dictionary<string, string> {{ "color", "red" }});
      var foo = (Dictionary<string, string>)metadata["foo"];
      Assert.AreEqual("baz", foo["bar"]);
      Assert.AreEqual("red", foo["color"]);
    }

    [Test]
    public void RemoveMetadataValues()
    {
      var metadata = new Metadata();
      metadata.Add("foo", new Dictionary<string, string> {{ "bar", "baz" }});
      metadata.Remove("foo");
      Assert.False(metadata.ContainsKey("foo"));
    }
  }
}
