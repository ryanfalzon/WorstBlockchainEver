using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SlightlyBetterBlockchain.Helper
{
    public static class Tools
    {
        public static bool AllowLogs { get; set; }

        public static string ToHex(byte[] data)
        {
            return String.Concat(data.Select(x => x.ToString("x2")));
        }

        public static byte[] GetSha256Hash(byte[] data)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(data);
            }
        }

        public static byte EncodeChar(char data)
        {
            return Convert.ToByte(data);
        }

        public static byte[] EncodeAscii(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public static byte[] EncodeUtf8(string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[] Encode(int data)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data));
        }

        public static byte[] Encode(long data)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data));
        }

        public static byte[] Encode(ushort data)
        {
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)data));
        }

        public static byte Encode(bool data)
        {
            return data ? (byte)1 : (byte)0;
        }

        public static string DecodeString(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static short DecodeInt16(byte[] data)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 0));
        }

        public static long DecodeInt32(byte[] data)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
        }

        public static long DecodeInt64(byte[] data)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
        }

        public static ushort DecodeUInt16(byte[] data)
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(data, 0));
        }

        public static bool DecodeBoolean(byte data)
        {
            return data == 1 ? true : false;
        }

        public static int GenerateAwaitTime(int minimum, int maximum)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(minimum, maximum);
        }

        public static long GetUnixTimestamp(DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeSeconds();
        }

        public static void Log(string message, bool overrideAllowLogs = false)
        {
            if (AllowLogs || overrideAllowLogs)
            {
                Console.WriteLine($"{DateTime.Now} - {message}");
            }
        }
    }
}