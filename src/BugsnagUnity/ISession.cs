using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface ISession
    {
        string Id { get; set; }
        IDevice Device { get; set; }
        IApp App { get; set; }
        DateTime? StartedAt { get; set; }
        User GetUser();
        void SetUser(string id, string email, string name);
    }
}
