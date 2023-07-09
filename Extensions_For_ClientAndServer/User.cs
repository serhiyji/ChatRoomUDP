using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Extensions_For_ClientAndServer
{
    public class User
    {
        public string Name { get; set; }
        public IPEndPoint endPoint { get; set; }
        public DateTime time { get; set; }
        public User(string name, IPEndPoint endPoint, DateTime time)
        {
            Name = name;
            this.endPoint = endPoint;
            this.time = time;
        }

        public override string ToString()
        {
            return $"User : {Name}, Ip : {endPoint.Address} / {endPoint.Port} / {time.ToLongTimeString()}";
        }
    }
}
