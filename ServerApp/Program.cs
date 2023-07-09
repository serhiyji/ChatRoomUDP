using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extensions_For_ClientAndServer;


namespace ServerApp
{

    class ChatServer
    {
        const short port = 4041;
        const int MAX_Users = 10;
        UdpClient server = new UdpClient(port);
        List<User> members = new List<User>();
        IPEndPoint clientEndPoint = null;
        private void AddMember(User user)
        {
            members.Add(user);
        }
        private async void SendToAll(byte[] data)
        {
            foreach (User member in members)
            {
                await server.SendAsync(data, data.Length, member.endPoint);
            }
        }
        private async void SendToSpecificUser(byte[] data, User user)
        {
            await server.SendAsync(data, data.Length, user.endPoint);
        }
        private async void SendToAllListUsers()
        {
            await Task.Run(() =>
            {
                IEnumerable<string> listusers = members.Select(i => i.Name).ToList();
                this.SendToAll(Encoding.Unicode.GetBytes(
                    new MessageInfo(MessageType.ListUsers, "Server", "All", "", Encoding.Unicode.GetBytes(listusers.ToBase64())).ToBase64()));
            });
        }
        private async void ClearAllUsers()
        {
            await Task.Run(() =>
            {
                string res1 = string.Join("\n", members.Where(i => i.time.AddMilliseconds(6500) < DateTime.Now));
                int res = members.RemoveAll(i => i.time.AddMilliseconds(6500) < DateTime.Now);
                if (res > 0)
                {
                    Console.WriteLine("remove -");
                    Console.WriteLine(res1);
                }
            });
        }
        private async void UpdateInfoFull()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    ClearAllUsers();
                    SendToAllListUsers();
                    Thread.Sleep(1000);
                }
            });
        }
        public void Start()
        {
            UpdateInfoFull();
            while (true)
            {
                try
                {
                    byte[] data = server.Receive(ref clientEndPoint);
                    string message = Encoding.Unicode.GetString(data);
                    MessageInfo info = message.FromBase64<MessageInfo>();
                    if (!members.Select(i => i.Name).Contains(info.FromNick) && info.type != MessageType.Connect) continue;
                    if (info.type != MessageType.Ping)
                    {
                        Console.WriteLine(info);
                    }
                    switch (info.type)
                    {
                        case MessageType.Public:
                            SendToAll(data);
                            break;
                        case MessageType.PublicData:
                            // ------
                            break;
                        case MessageType.Private:
                            User user_to = members.Where(i => i.Name == info.ToNick).FirstOrDefault();
                            User user_from = members.Where(i => i.Name == info.FromNick).FirstOrDefault();
                            if (user_to != null && user_from != null)
                            {
                                SendToSpecificUser(data, user_to);
                                SendToSpecificUser(data, user_from);
                            }
                            break;
                        case MessageType.PrivateData:
                            // ------
                            break;
                        case MessageType.Connect:
                            User NewUser = new User(info.FromNick, clientEndPoint, DateTime.Now);
                            if (members.Count() >= MAX_Users)
                            {
                                SendToSpecificUser(Encoding.Unicode.GetBytes(
                                    new MessageInfo(MessageType.Private, "Server", NewUser.Name, $"No more than {MAX_Users} people can connect to the server").ToBase64()),
                                    NewUser);
                            }
                            else
                            {
                                if (!members.Select(i => i.Name).Contains(NewUser.Name))
                                {
                                    AddMember(NewUser);
                                    MessageInfo connect_msg1 = new MessageInfo(MessageType.Public, info.FromNick, "All", $"User added to chat");
                                    SendToAll(Encoding.Unicode.GetBytes(connect_msg1.ToBase64()));
                                    Console.WriteLine($"Add {NewUser}");
                                }
                                else
                                {
                                    SendToSpecificUser(Encoding.Unicode.GetBytes(new MessageInfo(MessageType.Public, "Server", NewUser.Name, "The user under your nickname is on the server").ToBase64()), NewUser);
                                }
                            }
                            break;
                        case MessageType.Disconnect:
                            MessageInfo connect_msg2 = new MessageInfo(MessageType.Public, info.FromNick, "All", $"User deleted from the chat");
                            int res = members.RemoveAll(i => i.Name == info.FromNick);
                            if (res > 0)
                            {
                                SendToAll(Encoding.Unicode.GetBytes(connect_msg2.ToBase64()));
                            }
                            break;
                        case MessageType.ListUsers:
                            // ------
                            break;
                        case MessageType.GetPing:
                            // ------
                            break;
                        case MessageType.Ping:
                            User temp = members.Where(i => i.Name == info.FromNick).FirstOrDefault();
                            temp.time = DateTime.Now;
                            break;
                    }
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    //Console.WriteLine(ex.Message);
                }
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            ChatServer server = new ChatServer();
            server.Start();
        }
    }
}
