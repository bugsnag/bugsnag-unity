#if (UNITY_STANDALONE_OSX && !UNITY_EDITOR) || (UNITY_STANDALONE_OSX && BSG_COCOA_DEV)
namespace BugsnagUnity
{
    partial class NativeCode
    {
        const string Import = "bugsnag-osx";
    }
}
#endif