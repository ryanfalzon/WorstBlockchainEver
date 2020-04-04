using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Tools.Log($"Setting up client on {args[0]}:{args[1]}...");

            Client client = new Client(new Node()
            {
                IPAddress = IPAddress.Parse(args[0]),
                Port = Convert.ToInt32(args[1])
            });

            Tools.Log($"Client setup completed!");

            client.InitPeers();

            Task processIncomingMessages = Task.Factory.StartNew(() =>
            {
                client.ProcessIncomingMessages();
            });

            Task scheduleNextMessage = Task.Factory.StartNew(() =>
            {
                ScheduleNextMessage(client);
            });

            Task.WaitAll(processIncomingMessages, scheduleNextMessage);
        }

        public static void ScheduleNextMessage(Client client)
        {
            while (true)
            {
                int milliseconds = Tools.GenerateAwaitTime(500, 5000);
                Tools.Log($"Next message will be broadcasted in {milliseconds}ms...");
                Thread.Sleep(milliseconds);
                client.BroadcastMessage("Test");
            }
        }
    }
}