using Extensions_For_ClientAndServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;

namespace ClientApp
{
    [AddINotifyPropertyChangedInterface]
    public partial class ViewModel
    {
        public string MessagePublic { get; set; }
        public string MessagePrivate { get; set; }
        public string Nick { get; set; }

        private RelayCommand disconnectedCommand;
        private RelayCommand connectedCommand;
        private RelayCommand sendPublicMessageCommand;
        private RelayCommand sendPrivateMessageCommand;

        public ICommand disconnectedCmd => disconnectedCommand;
        public ICommand connectedCmd => connectedCommand;
        public ICommand sendPublicMessageCmd => sendPublicMessageCommand;
        public ICommand sendPrivateMessageCmd => sendPrivateMessageCommand;

        public bool IsConnected { get; set; }

        private IPEndPoint serverEndPoint;
        private UdpClient client;
        private ObservableCollection<MessageInfo> messagesPublic;
        private ObservableCollection<UserInfo> usersInfo;
        public IEnumerable<MessageInfo> MessagesPublic => messagesPublic;
        public IEnumerable<UserInfo> Users => usersInfo;

        private UserInfo selectedItemUser;
        public UserInfo SelectedItemUser
        {
            get { return selectedItemUser; }
            set
            {
                if (value != null)
                {
                    this.selectedItemUser = value;
                }
            }
        }
    }
}
