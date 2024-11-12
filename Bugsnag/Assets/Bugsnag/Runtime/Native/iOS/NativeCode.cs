#if (UNITY_IOS && !UNITY_EDITOR) || BSG_COCOA_DEV
namespace BugsnagUnity
{
    partial class NativeCode
    {
        const string Import = "__Internal";
    }
}
#endif