using System;
using System.Text;

namespace WorstBlockchainEver.Helper
{
    public static class Tools
    {
        public static bool AllowLogs { get; set; }

        public static byte[] Encode(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public static byte[] Encode(long data)
        {
            return BitConverter.GetBytes(data);
        }

        public static byte[] Encode(ushort data)
        {
            return BitConverter.GetBytes(data);
        }

        public static string DecodeString(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static long DecodeInt64(byte[] data)
        {
            return BitConverter.ToInt64(data, 0);
        }
        
        public static ushort DecodeUInt16(byte[] data)
        {
            return BitConverter.ToUInt16(data, 0);
        }

        public static int GenerateAwaitTime(int minimum, int maximum)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(minimum, maximum);
        }

        public static long GetUnixTimestamp(DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeMilliseconds();
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