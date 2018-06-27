using System;

namespace Bugsnag.Polyfills
{
  static class String
  {
    internal static bool IsNullOrWhiteSpace(string s)
    {
      if (s == null) return true;

      for (int i = 0; i < s.Length; i++)
      {
        if (!Char.IsWhiteSpace(s[i])) return false;
      }

      return true;
    }
  }
}
