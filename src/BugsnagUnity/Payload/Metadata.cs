using System.Collections.Generic;
using BugsnagUnity;

namespace BugsnagUnity.Payload
{
  public class Metadata : Dictionary<string, object>, IFilterable
  {
    private INativeClient NativeClient = null;

    public Metadata() {
    }

    internal Metadata(INativeClient nativeClient) {
      NativeClient = nativeClient;
    }

    public void Add(string section, object newValue) {
      if (NativeClient != null) {
        if (newValue is Dictionary<string, string> stringValues) {
          base.Add(section, stringValues);
          NativeClient.SetMetadata(section, stringValues);
        } else if (newValue is Dictionary<string, object> objectValues) {
          var target = new Dictionary<string, string>();
          foreach(var pair in objectValues) {
            target.Add(pair.Key, pair.Value.ToString());
          }
          base.Add(section, target);
          NativeClient.SetMetadata(section, target);
        }
      } else {
        base.Add(section, newValue);
      }
    }

    public void Remove(string section) {
      base.Remove(section);
      if (NativeClient != null) {
        NativeClient.SetMetadata(section, null);
      }
    }
  }
}
