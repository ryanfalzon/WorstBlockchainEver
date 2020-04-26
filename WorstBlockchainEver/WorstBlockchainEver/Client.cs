using System;
using System.Linq;
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
            Tools.AllowLogs = true;

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
            Client.State = new State
            {
                CurrentState = States.Synchronized
            };

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

            // Open a new thread to award WBE to user
            Task awardWbe = Task.Factory.StartNew(() =>
            {
                AwardWbe();
            });

            Tools.Log($"Client setup completed!");

            Task.WaitAll(processIncomingMessages, scheduleNextSyncTest, userActions, awardWbe);
        }

        public static void AwardWbe()
        {
            Thread.Sleep(5000);
            for (int i = 0; i < 10; i++)
            {
                State.AddLocalTransaction(new Transaction()
                {
                    Number = Convert.ToUInt16(State.Transactions.Count + 1),
                    Time = Tools.GetUnixTimestamp(DateTime.Now),
                    From = "00",
                    To = User
                });

                i++;
            }
        }

        public static void ScheduleNextSyncTest(Peers peers)
        {
            Thread.Sleep(Tools.GenerateAwaitTime(1000, 5000));
            while (true)
            {
                // Node needs to be in Synchronized state
                while(State.CurrentState != States.Synchronized)
                {
                    Thread.Sleep(1000);
                }

                // Request highest transaction number from the network
                Tools.Log("Sync process initializing...");
                peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransaction));
                State.SyncCounter = 0;
                State.CurrentState = States.InitializingSyncProcess;

                // Node needs to be removed from the InitializingSyncProcess or Syncing state to continue
                while (State.CurrentState == States.InitializingSyncProcess || State.CurrentState == States.Syncing)
                {
                    Tools.Log("Syncing...");
                    Thread.Sleep(1000);
                }

                // If state is not synchronized, then we will request the missing transactions
                if(State.CurrentState == States.NotSynchronized)
                {
                    Tools.Log("Node is not synced! Requesting transactions...");
                    State.CurrentState = States.AwaitingTransactions;
                    State.Synchronize();
                    State.CurrentState = States.Synchronized;
                }
                else
                {
                    Tools.Log("Node synced successfully!");
                }

                Tools.Log($"Next sync process will occur in {Properties.Settings.Default.SyncDelay}...");
                Thread.Sleep(Properties.Settings.Default.SyncDelay);
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
                    Tools.AllowLogs = false;

                    Console.WriteLine("Actions");
                    Console.WriteLine("-------");
                    Console.WriteLine("1) Check Balance");
                    Console.WriteLine("2) Send WBE");
                    Console.WriteLine("3) Exii\n");
                    Console.Write("Choice: ");

                    var choice = Convert.ToInt32(Console.ReadLine());

                    switch (choice)
                    {
                        case 1: CheckBalance(); break;
                        case 2: SendWBE(); break;
                        case 3: Environment.Exit(0); break;
                        default: Console.WriteLine("Invalid Entry..."); break;
                    }

                    Tools.AllowLogs = true;
                }
            }
        }

        private static void CheckBalance()
        {
            Console.Write("Enter user initials: ");
            string user = Console.ReadLine();
            Tools.Log($"Balance for user {user}: {State.CheckBalance(user)}", true);
        }

        private static void SendWBE()
        {
            Console.Write("Enter user initials: ");
            string to = Console.ReadLine();

            Tools.AllowLogs = true;

            var transactionNumber = State.Transactions.Count == 0 ? 1 : State.Transactions.Last().Number + 1;

            State.AddLocalTransaction(new Transaction()
            {
                Number = Convert.ToUInt16(transactionNumber),
                Time = Tools.GetUnixTimestamp(DateTime.Now),
                From = Client.User,
                To = to
            });
        }
    }
}