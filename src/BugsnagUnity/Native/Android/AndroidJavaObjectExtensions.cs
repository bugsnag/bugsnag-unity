using UnityEngine;

namespace BugsnagUnity
{
  static class AndroidJavaObjectExtensions
  {
    /// <summary>
    /// Calls the string method on the android java object. When the return value
    /// is null, then the JNI layer does not handle returning a string so we need
    /// to handle this manually here.
    /// </summary>
    /// <returns>The string method.</returns>
    /// <param name="object">Object.</param>
    /// <param name="methodName">Method name.</param>
    public static string CallStringMethod(this AndroidJavaObject @object, string methodName)
    {
      var value = @object.Call<AndroidJavaObject>(methodName);

      if (value != null)
      {
        return AndroidJNI.GetStringUTFChars(value.GetRawObject());
      }

      return null;
    }
  }
}
