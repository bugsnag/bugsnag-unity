using System;

namespace Bugsnag.Polyfills
{
  public static class String
  {
    public static bool IsNullOrWhiteSpace(string s)
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
