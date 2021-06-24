using System.Collections.Generic;
using System.ComponentModel;

namespace BugsnagUnity.Payload
{
    public class User : Dictionary<string, string>, IFilterable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal User()
        {

        }

        public string Id
        {
            get
            {
                return this.Get("id");
            }
            set
            {
                this.AddToPayload("id", value);
                OnPropertyChanged("Id");
            }
        }

        public string Name
        {
            get
            {
                return this.Get("name");
            }
            set
            {
                this.AddToPayload("name", value);
                OnPropertyChanged("Name");
            }
        }

        public string Email
        {
            get
            {
                return this.Get("email");
            }
            set
            {
                this.AddToPayload("email", value);
                OnPropertyChanged("Email");
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            // Inform the Client when its user changes a property as it will need to be
            // synchronized with the native layer
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
