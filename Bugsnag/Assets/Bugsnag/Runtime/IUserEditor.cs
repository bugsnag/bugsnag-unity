using System;
namespace BugsnagUnity
{
    public interface IUserEditor
    {
        IUser GetUser();

        void SetUser(string id, string email, string name);
    }
}
