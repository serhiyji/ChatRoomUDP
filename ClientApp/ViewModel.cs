using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Input;
using Extensions_For_ClientAndServer;
using PropertyChanged;
using System.Windows;
using System.Threading;
using System.Windows.Controls;

namespace ClientApp
{
    public partial class ViewModel
    {
        public ViewModel()
        {
            IsConnected = false;
            serverEndPoint = null;
            selectedItemUser = null;
            client = null;
            SelectedItemUser = null;

            disconnectedCommand = new RelayCommand((o) => LeaveBtnClick(), (o) => IsConnected);
            connectedCommand = new RelayCommand((o) => JoinBtnClick(), (o) => !IsConnected && Nick != "");
            sendPublicMessageCommand = new RelayCommand((o) => SendPublicBtnClick(), (o) => IsConnected && MessagePublic != "");
            sendPrivateMessageCommand = new RelayCommand((o) => SendPrivateBtnClick(), (o) => IsConnected && SelectedItemUser?.Name != null && MessagePrivate != "");

            messagesPublic = new ObservableCollection<MessageInfo>();
            usersInfo = new ObservableCollection<UserInfo>();
            SetDefoltStringProp();
        }
        ~ViewModel()
        {
            try
            {
                client.Close();
            }
            catch (Exception) { }
        }
        private void SetDefoltStringProp()
        {
            MessagePublic = "";
            MessagePrivate = "";
            Nick = "";
            SelectedItemUser = null;
            SelectedItemUser?.messages.Clear();
            messagesPublic.Clear();
            usersInfo.Clear();
        }
        private void LeaveBtnClick()
        {
            SendMessage(new MessageInfo(MessageType.Disconnect, Nick, "Server").ToBase64());
            SetDefoltStringProp();
            serverEndPoint = null;
            IsConnected = false;
        }
        private void SendPublicBtnClick()
        {
            SendMessage(new MessageInfo(MessageType.Public, Nick, "All", MessagePublic).ToBase64());
            MessagePublic = "";
        }
        private void SendPrivateBtnClick()
        {
            SendMessage(new MessageInfo(MessageType.Private, Nick, SelectedItemUser?.Name, MessagePrivate).ToBase64());
            MessagePrivate = "";
        }
        private async void SendPing()
        {
            await Task.Run(() =>
            {
                try
                {
                    while (IsConnected)
                    {
                        SendMessage(new MessageInfo(MessageType.Ping, Nick).ToBase64());
                        Thread.Sleep(3000);
                    }   
                }
                catch (Exception)
                {
                    IsConnected = false;
                }
            });
        }
        private void JoinBtnClick()
        {
            try
            {
                string serverAddress = ConfigurationManager.AppSettings["ServerAdress"]!;
                short serverPort = short.Parse(ConfigurationManager.AppSettings["ServerPort"]!);
                serverEndPoint = new IPEndPoint(IPAddress.Parse(serverAddress), serverPort);
                client = new UdpClient();
                SendMessage(new MessageInfo(MessageType.Connect, Nick).ToBase64());
                IsConnected = true;
                Listen();
                SendPing();
            }
            catch (Exception) { }
        }
        private async void UsersListHandler(byte[] users_in)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    List<string> newusers = Encoding.Unicode.GetString(users_in).FromBase64<List<string>>().Where(i => i != Nick).ToList();
                    List<string> current_inp = usersInfo.Select(i => i.Name).ToList();
                    List<string> removeusers = current_inp.Where(i => !newusers.Contains(i)).ToList();
                    foreach (string item in removeusers)
                    {
                        usersInfo.Remove(usersInfo.Where(i => i.Name == item).First());
                    }
                    newusers.RemoveAll(i => current_inp.Contains(i));
                    foreach (string item in newusers)
                    {
                        usersInfo.Add(new UserInfo(item));
                    }
                });
            });
        }
        private async void PrivateMessageHandler(MessageInfo info)
        {
            string temp_n = (Nick == info.FromNick) ? info.ToNick : info.FromNick;
            usersInfo.FirstOrDefault(i => i.Name == temp_n)?.messages.Add(info);
        }
        private async void Listen()
        {
            try
            {
                while (IsConnected)
                {
                    var res = await client.ReceiveAsync();
                    string message = Encoding.Unicode.GetString(res.Buffer);
                    MessageInfo info = message.FromBase64<MessageInfo>();
                    switch (info.type)
                    {
                        case MessageType.Public:
                            messagesPublic.Add(info);
                            break;
                        case MessageType.PublicData:
                            break;
                        case MessageType.Private:
                            this.PrivateMessageHandler(info);
                            break;
                        case MessageType.PrivateData:
                            break;
                        case MessageType.Connect:
                            break;
                        case MessageType.Disconnect:
                            LeaveBtnClick();
                            break;
                        case MessageType.ListUsers:
                            this.UsersListHandler(info.Data);
                            break;
                        case MessageType.GetPing:
                            break;
                        case MessageType.Ping:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsConnected = false;
            }
        }
        private async void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            await client.SendAsync(data, data.Length, serverEndPoint);
        }
    }
}
