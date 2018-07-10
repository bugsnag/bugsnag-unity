using System.Collections.Generic;

namespace Bugsnag.Unity.Payload
{
  static class PayloadExtensions
  {
    /// <summary>
    /// Adds a key to the Bugsnag payload. If provided a null or empty string
    /// value will remove the key from the dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    internal static void AddToPayload<T>(this Dictionary<string, T> dictionary, string key, T value)
    {
      if (value == null)
      {
        dictionary.Remove(key);
        return;
      }

      switch (value)
      {
        case System.String s:
          if (!Polyfills.String.IsNullOrWhiteSpace(s))
          {
            dictionary[key] = value;
          }
          else if (dictionary.ContainsKey(key))
          {
            dictionary.Remove(key);
          }
          break;
        default:
          dictionary[key] = value;
          break;
      }
    }

    internal static U Get<T, U>(this Dictionary<T, U> dictionary, T key)
    {
      dictionary.TryGetValue(key, out U value);
      return value;
    }
  }
}
