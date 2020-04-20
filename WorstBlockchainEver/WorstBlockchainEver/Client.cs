﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public static class Client
    {
        public static Peers Peers { get; set; }

        public static State State { get; set; }

        public static string User { get; set; }

        public static void Main(string[] args)
        {
            Tools.Log($"Setting up client on {args[0]}:{args[1]} for user {args[2]}...");

            Client.User = args[2];

            // Create peer list for this client and initialize them
            Client.Peers = new Peers(new Node()
            {
                IPAddress = IPAddress.Parse(args[0]),
                Port = Convert.ToInt32(args[1])
            });
            Client.Peers.InitPeers();

            // Create new state for this client
            Client.State = new State();

            // Open a new thread to process incoming messages
            Task processIncomingMessages = Task.Factory.StartNew(() =>
            {
                Client.Peers.ProcessIncomingMessages();
            });

            // Open a new thread to schedule a next synchronize call
            Task scheduleNextSyncTest = Task.Factory.StartNew(() =>
            {
                ScheduleNextSyncTest(Client.Peers);
            });

            // Open a new thread to handle the user action in the console
            Task userActions = Task.Factory.StartNew(() =>
            {
                HandleUserActions();
            });

            // Sync node with other nodes in the network
            Client.State.Synchronize();

            Task.WaitAll(processIncomingMessages, scheduleNextSyncTest, userActions);

            Tools.Log($"Client setup completed!");
        }

        public static void ScheduleNextSyncTest(Peers peers)
        {
            while (true)
            {
                Tools.Log($"Checking highest transaction in network in 5000ms...");
                Thread.Sleep(5000);
                peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransaction));
            }
        }

        public static void HandleUserActions()
        {
            Console.WriteLine("Press 'C' to enter in command input...");

            ConsoleKeyInfo keyInfo;

            while (true)
            {
                keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == ConsoleKey.C)
                {
                    Console.WriteLine("Actions");
                    Console.WriteLine("-------");
                    Console.WriteLine("1) Check Balance");
                    Console.WriteLine("2) Send WBE\n");
                    Console.Write("Choice: ");

                    var choice = Convert.ToInt32(Console.ReadLine());

                    switch (choice)
                    {
                        case 1: CheckBalance(); break;
                        case 2: SendWBE(); break;
                        default: Console.WriteLine("Invalid Entry..."); break;
                    }
                }
            }
        }

        private static void CheckBalance()
        {
            Console.Write("Enter user initials: ");
            string user = Console.ReadLine();
            Tools.Log($"Balance for user {user}: {State.CheckBalance(user)}");
        }

        private static void SendWBE()
        {
            Console.Write("Enter user initials: ");
            string to = Console.ReadLine();
            State.AddLocalTransaction(new Transaction()
            {
                Number = Convert.ToUInt16(State.Transactions.Count + 1),
                Time = Tools.GetUnixTimestamp(DateTime.Now),
                From = Client.User,
                To = to
            });
        }
    }
}