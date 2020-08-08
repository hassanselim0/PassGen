using System;
using System.Text;

namespace PassGenCore
{
    public static class Extensions
    {
        public static byte[] ToUTF8(this string data) => Encoding.UTF8.GetBytes(data);

        public static string ToBase64(this byte[] data) => Convert.ToBase64String(data);

        public static byte[] FromBase64(this string data) => Convert.FromBase64String(data);
    }
}