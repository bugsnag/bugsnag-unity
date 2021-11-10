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

        public string Id { get => GetNativeString(ID_KEY); set => SetNativeString(ID_KEY,value); }
        public string Name { get => GetNativeString(NAME_KEY); set => SetNativeString(NAME_KEY, value); }
        public string Email { get => GetNativeString(EMAIL_KEY); set => SetNativeString(EMAIL_KEY, value); }
    }
}
