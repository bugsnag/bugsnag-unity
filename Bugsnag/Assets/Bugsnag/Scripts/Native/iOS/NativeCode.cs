#if (UNITY_IOS && !UNITY_EDITOR) || BGS_COCOA_DEV
namespace BugsnagUnity
{
    partial class NativeCode
    {
        const string Import = "__Internal";
    }
}
#endif