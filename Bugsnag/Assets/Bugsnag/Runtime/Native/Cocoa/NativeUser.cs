#if ((UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR) || BSG_COCOA_DEV

using System;
namespace BugsnagUnity
{
    public class NativeUser : NativePayloadClassWrapper, IUser
    {

        private const string ID_KEY = "id";
        private const string NAME_KEY = "name";
        private const string EMAIL_KEY = "name";


        public NativeUser(IntPtr nativeUser) : base (nativeUser)
        {
        }

        public string Id => GetNativeString(ID_KEY);
        public string Name => GetNativeString(NAME_KEY);
        public string Email => GetNativeString(EMAIL_KEY);
    }
}
#endif