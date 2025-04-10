using System;
namespace BugsnagUnity
{
    public interface IUser
    {
        string Id { get; }

        string Name { get; }

        string Email { get; }
    }
}
