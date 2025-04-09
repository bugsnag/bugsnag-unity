using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Events;
#nullable enable

namespace BugsnagUnity.Payload
{
    public class User : PayloadContainer, IUser
    {
        internal UnityEvent PropertyChanged = new UnityEvent();

        private const string ID_KEY = "id";
        private const string NAME_KEY = "name";
        private const string EMAIL_KEY = "email";


        internal User()
        {

        }

        public User(string id, string email, string name)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public string Id
        {
            get
            {
                var @object = Get(ID_KEY);
                if (@object == null)
                {
                    return string.Empty;
                }
                return (string)@object;
            }
            set
            {
                Add(ID_KEY, value);
                PropertyChanged.Invoke();
            }
        }

        public string Name
        {
            get
            {
                var @object = Get(NAME_KEY);
                if (@object == null)
                {
                    return string.Empty;
                }
                return (string)@object;
            }
            set
            {
                Add(NAME_KEY, value);
                PropertyChanged.Invoke();
            }
        }

        public string Email
        {
            get
            {
                var @object = Get(EMAIL_KEY);
                if (@object == null)
                {
                    return string.Empty;
                }
                return (string)@object;
            }
            set
            {
                Add(EMAIL_KEY, value);
                PropertyChanged.Invoke();
            }
        }

        internal User Clone()
        {
            return (User)MemberwiseClone();
        }
    }
}
