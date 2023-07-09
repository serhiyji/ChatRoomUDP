using System;

namespace Extensions_For_ClientAndServer
{
    public enum MessageType { Public, PublicData, Private, PrivateData, Connect, Disconnect, ListUsers, GetPing, Ping };
    public partial class MessageInfo
    {
        public MessageType type { get; set; }
        public string FromNick { get; set; }
        public string ToNick { get; set; }
        public string Message { get; set; }
        public byte[] Data { get; set; }
        public DateTime Time { get; set; }
        public MessageInfo(MessageType type, string fromNick, string toNick = "/", string message = "/", byte[] data = null)
        {
            this.type = type;
            FromNick = fromNick;
            ToNick = toNick;
            Message = message;
            Data = data;
            Time = DateTime.Now;
        }
        public override string ToString()
        {
            return $"type:{type}, {FromNick} -> {ToNick}, Message : {Message}, Time:{Time}";
        }
    }
    // For View
    public partial class MessageInfo
    {
        public string _TimeSend => Time.ToLongTimeString();
        public string _Message => Message;
    }
}
