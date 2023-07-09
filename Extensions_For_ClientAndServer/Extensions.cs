using System;
using System.Text;
using Newtonsoft.Json;

namespace Extensions_For_ClientAndServer
{
    public static class Extensions
    {
        public static string ToBase64(this object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            byte[] bytes = Encoding.Unicode.GetBytes(json);
            return Encoding.Unicode.GetString(bytes);
        }
        public static T FromBase64<T>(this string base64Text)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(base64Text);
            string json = Encoding.Unicode.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
