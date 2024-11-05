#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
namespace BugsnagUnity
{
    partial class NativeCode
    {
        const string Import = "__Internal";
    }
}
#endif