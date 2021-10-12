using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Events;
#nullable enable

namespace BugsnagUnity.Payload
{
    public class User : PayloadContainer
    {
        internal UnityEvent PropertyChanged = new UnityEvent();

        private const string ID_KEY = "id";
        private const string NAME_KEY = "name";
        private const string EMAIL_KEY = "email";


        internal User()
        {

        }

        public User(string? id, string? name, string? email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public string? Id
        {
            get => (string?)Get(ID_KEY);
            set
            {
                Add(ID_KEY, value);
                PropertyChanged.Invoke();
            }  
        }

        public string? Name
        {
            get => (string?)Get(NAME_KEY);
            set
            {
                Add(NAME_KEY, value);
                PropertyChanged.Invoke();
            }
        }

        public string? Email
        {
            get => (string?)Get(EMAIL_KEY);
            set
            {
                Add(EMAIL_KEY, value);
                PropertyChanged.Invoke();
            }
        }
    }
}
