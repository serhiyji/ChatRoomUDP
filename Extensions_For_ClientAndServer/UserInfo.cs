using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions_For_ClientAndServer
{
    [AddINotifyPropertyChangedInterface]
    public class UserInfo
    {
        public string Name { get; set; }
        public ObservableCollection<MessageInfo> messages { get; set; }
        public int? CountNewMessages { get; set; }
        public UserInfo(string name)
        {
            Name = name;
            this.messages = new ObservableCollection<MessageInfo>();
            CountNewMessages = 0;
        }
    }
}
