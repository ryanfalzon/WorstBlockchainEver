﻿using System;
using System.Net;
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