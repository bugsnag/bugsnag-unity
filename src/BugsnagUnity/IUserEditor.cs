using System;
namespace BugsnagUnity
{
    public interface IUserEditor
    {
        Payload.User GetUser();

        void SetUser(string id, string email, string name);
    }
}
