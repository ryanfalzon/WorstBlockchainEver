using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using System;
using System.Threading.Tasks;

namespace SlightlyBetterBlockchain
{
    /* Problems with current implementation
     *  - Everytime the client is started, a differnt public and private key pair are generated
     *  - No way to communicate with other nodes in the network
     *  - COnfirmation block not yet implemeted
     */
    public class Client
    {
        public static Wallet Wallet { get; set; }

        public static Chain Chain { get; set; }

        public static Miner Miner { get; set; }

        public static void Main(string[] args)
        {
            Tools.AllowLogs = true;

            // Create wallet
            Client.Wallet = new Wallet();

            // Create chain
            Client.Chain = new Chain();

            // Create miner object
            Client.Miner = new Miner();

            // Start mining process thread
            Task mining = Task.Factory.StartNew(() =>
            {
                Client.Miner.Run();
            });

            // Open a new thread to handle the user action in the console
            Task userActions = Task.Factory.StartNew(() =>
            {
                HandleUserActions();
            });

            Tools.Log($"Client setup completed!");

            Task.WaitAll(mining, userActions);
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
                    Console.WriteLine("3) Exit\n");
                    Console.Write("Choice: ");

                    try
                    {
                        var choice = Convert.ToInt32(Console.ReadLine());

                        switch (choice)
                        {
                            case 1: CheckBalance(); break;
                            case 2: SendWBE(); break;
                            case 3: Environment.Exit(0); break;
                            default: Console.WriteLine("Invalid Entry..."); break;
                        }
                    }
                    catch
                    {
                        Tools.AllowLogs = true;
                        Tools.Log("An error occured while parsing choice!");
                    }
                    finally
                    {
                        Tools.AllowLogs = true;
                    }
                }
            }
        }

        private static void CheckBalance()
        {
            Tools.Log($"Balance: {Wallet.CheckBalance()}", true);
        }

        /* Checks that need to happen prior to sending SBB
         *  - Check that the address the user enters is a valid ethereum address
         *  - Check that the user has enough SBB by counting the ins and outs from the current chain
         *  - Check how many transactions the user has in the mining pool
         *  - Take into consideration the transactions which have not passed the confirmation block yet
         */
        private static void SendWBE()
        {
            Console.Write("Enter receiver public key: ");
            string to = Console.ReadLine();

            if(Wallet.CheckBalance() >= 1)
            {
                Tools.AllowLogs = true;

                var transaction = new Transaction()
                {
                    From = Wallet.GetPublicKey(),
                    To = to
                };
                transaction.CalculateHash();
                Miner.MiningPool.Add(transaction);
            }
            else
            {
                Tools.Log("Insufficient balance!");
            }
        }
    }
}
 