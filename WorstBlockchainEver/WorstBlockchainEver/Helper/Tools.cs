using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorstBlockchainEver.Helper
{
    public static class Tools
    {
        public static byte[] EncodeMessage(string message)
        {
            return Encoding.ASCII.GetBytes(message);
        }

        public static string DecodeMessage(byte[] messageBytes)
        {
            return Encoding.ASCII.GetString(messageBytes);
        }

        public static int GenerateAwaitTime(int minimum, int maximum)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            return random.Next(minimum, maximum);
        }

        public static void Log(string message)
        {
            Console.WriteLine($"{DateTime.Now} - {message}");
        }
    }
}
