﻿using System;
using BugsnagUnity.Payload;

namespace BugsnagUnity
{
    public interface ISession
    {
        string Id { get; set; }
        Device Device { get; set; }
        App App { get; set; }
        DateTime? StartedAt { get; set; }
        User GetUser();
        void SetUser(string id, string email, string name);
    }
}
