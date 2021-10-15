using System;
namespace BugsnagUnity
{
    internal interface IUserEditor
    {
        Payload.User GetUser();

        void SetUser(string id, string email, string name);
    }
}
