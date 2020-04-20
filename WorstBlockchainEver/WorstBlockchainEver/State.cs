using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class State
    {
        public bool Synchronized { get; set; }

        public bool Syncing { get; set; }

        public List<Transaction> Transactions{ get; set; }

        public int NetworkTransactionCount { get; set; }

        public State()
        {
            this.Transactions = new List<Transaction>();
            this.NetworkTransactionCount = 0;
            this.Synchronized = false;
            this.Syncing = false;
        }

        public void Synchronize()
        {
            Thread.Sleep(5000);
            // Broadcast message to get the highest transaction number that the other nodes have
            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransaction));

            // Wait until this node is synced with all other nodes
            while (true)
            {
                if (this.Synchronized)
                {
                    // Node has synced successfully
                    Tools.Log("Node synced successfully!");
                    break;
                }
                else
                {
                    // Node is still syncing
                    Tools.Log("Syncing...");
                    Thread.Sleep(1000);
                }
            }

            this.Synchronized = true;

            // Award 10WBE
            for(int i = 0; i < 10; i++)
            {
                this.AddLocalTransaction(new Transaction()
                {
                    Number = Convert.ToUInt16(this.Transactions.Count + 1),
                    Time = Tools.GetUnixTimestamp(DateTime.Now),
                    From = "0x",
                    To = Client.User
                });
            }
        }

        public void GetTransaction(ushort number)
        {
            this.Syncing = true;
            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.GetTransaction, new string[] { number.ToString() }));
        }

        public void AddLocalTransaction(Transaction transaction)
        {
            // Check if user has sufficient balance or the user is the 0x address
            if(CheckBalance(transaction.From) >= 1 || transaction.From.Equals("0x"))
            {
                Tools.Log($"Creating new transaction with number {transaction.Number}...");

                // Add transaction to local chain and broadcast new transactionm to network
                this.Transactions.Add(transaction);
                Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NewTransaction, new string[]
                {
                    transaction.Number.ToString(),
                    transaction.Time.ToString(),
                    transaction.From,
                    transaction.To
                }));

                Tools.Log($"Transaction with number {transaction.Number} created successfully and was broadcasted to network");
            }
            else
            {
                Tools.Log($"Could not create transaction as user {transaction.From} has insufficient balance!");
            }
        }

        public void AddNetworkTransaction(Transaction transaction)
        {
            Tools.Log($"New transaction with number {transaction.Number} received...");

            var transactionExists = this.Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault();
            if(transactionExists != null)
            {
                Tools.Log($"Transaction with number {transaction.Number} already exists...");

                if(transaction.Time <= transactionExists.Time)
                {
                    Tools.Log($"Transaction received is older...");
                    ReplaceTransaction(transaction);
                }
                else
                {
                    Tools.Log($"Transaction received will be discarded as it is older...");
                }
            }
            else
            {
                this.Transactions.Add(transaction);
                Tools.Log($"Transaction with number {transaction.Number} added successfully!");
            }
        }

        public void ReplaceTransaction(Transaction transaction)
        {
            Tools.Log($"Replacing received transaction with number {transaction.Number}...");
            this.Transactions.Insert(Transactions.IndexOf(Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault()), transaction);
            Tools.Log($"Transaction with number {transaction.Number} replaced successfully!");
        }

        public long CheckBalance(string user)
        {
            return this.Transactions.Where(transaction => transaction.To.Equals(user)).Count() - Transactions.Where(transaction => transaction.From.Equals(user)).Count();
        }
    }
}