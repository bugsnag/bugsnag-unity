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
      if (ContainsKey(section)) {
        var currentValue = this[section];
        base.Remove(section);
        if (currentValue is Dictionary<string, string> currentStringValues) {
          if (newValue is Dictionary<string, string> newStringValues) {
            base.Add(section, MergeDicts(currentStringValues, newStringValues));
          } else if (newValue is Dictionary<string, object> newObjectValues) {
            base.Add(section, MergeDicts(currentStringValues, CoerceDictType(newObjectValues)));
          } else {
            AddValueAsDict(section, newValue);
          }
        } else {
          AddValueAsDict(section, newValue);
        }
      } else {
        AddValueAsDict(section, newValue);
      }
      if (NativeClient != null && this[section] is Dictionary<string, string> stringValues) {
          NativeClient.SetMetadata(section, stringValues);
      }
    }

    public void Remove(string section) {
      base.Remove(section);
      if (NativeClient != null) {
        NativeClient.SetMetadata(section, null);
      }
    }

    private void AddValueAsDict(string section, object input) {
      if (input is Dictionary<string, string> newInput) {
        base.Add(section, newInput);
      } else if (input is Dictionary<string, object> newObjectValue) {
        base.Add(section, CoerceDictType(newObjectValue));
      } else {
        var target = new Dictionary<string, string>();
        target.Add(section, input.ToString());
        Add("custom", target);
      }
    }

    private Dictionary<string, string> CoerceDictType(Dictionary<string, object> input) {
      var target = new Dictionary<string, string>();
      foreach(var pair in input) {
        target.Add(pair.Key, pair.Value.ToString());
      }
      return target;
    }

    private Dictionary<string, string> MergeDicts(Dictionary<string, string> input, 
                                                  Dictionary<string, string> overrides) {
      var target = new Dictionary<string, string>(input);
      foreach (var pair in overrides) {
        if (target.ContainsKey(pair.Key)) {
          target.Remove(pair.Key);
        }
        target.Add(pair.Key, pair.Value);
      }
      return target;
    }
  }
}
