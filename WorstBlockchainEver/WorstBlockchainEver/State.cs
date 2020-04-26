using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class State
    {
        public States CurrentState { get; set; }

        public List<Transaction> Transactions{ get; set; }

        public int NetworkTransactionCount { get; set; }

        public int SyncCounter { get; set; }

        public State()
        {
            this.Transactions = new List<Transaction>();
            this.SyncCounter = 0;
        }

        public bool IsSynced()
        {
            return this.Transactions.Count == this.NetworkTransactionCount;
        }

        public void Synchronize()
        {
            // Get all transactions that are not present locally by asking other nodes for them
            for (int i = this.Transactions.Count; i <= this.NetworkTransactionCount; i++)
            {
                // We do not need transaction with number 0
                if (i != 0)
                {
                    this.GetTransaction((ushort)i);
                }
            }

            // Wait until all transaction have been received
            while(this.Transactions.Count != this.NetworkTransactionCount)
            {
                Thread.Sleep(1000);
            }
        }

        public void GetTransaction(ushort number)
        {
            Tools.Log($"Requesting transaction with number {number} from network...");
            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.GetTransaction, new string[] { number.ToString() }));
        }

        public void AddLocalTransaction(Transaction transaction)
        {
            // Node needs to be in a synchronized state to continue
            while (this.CurrentState != States.Synchronized)
            {
                Thread.Sleep(1000);
            }

            // Set current state to sending transaction
            this.CurrentState = States.SendingTransaction;

            // Check if user has sufficient balance or the user is the 0x address
            if (CheckBalance(transaction.From) >= 1 || transaction.From.Equals("00"))
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

            // Set current state to synchronized
            this.CurrentState = States.Synchronized;
        }

        public void AddNetworkTransaction(Transaction transaction)
        {
            Tools.Log($"New transaction with number {transaction.Number} received...");

            // Check if transaction with same number exists in the local transaction chain
            var transactionExists = this.Transactions.Exists(t => t.Number == transaction.Number);
            if (transactionExists)
            {
                Tools.Log($"Transaction with number {transaction.Number} already exists...");
                var existingTransaction = this.Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault();

                // Check if the newly received transaction is older than the one present in local transaction chain
                if (transaction.Time <= existingTransaction.Time)
                {
                    // Replace the newly received transaction with the one already present in local transaction chain
                    Tools.Log($"Transaction received is older...");
                    ReplaceTransaction(transaction);
                }
                else
                {
                    // Discrard newly received transaction as it is newer
                    Tools.Log($"Transaction received will be discarded as it is older...");
                }
            }
            else
            {
                // Add transaction to local transaction chain
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
            // Coujnt all the ins and deduct the outs from the result (Same concept bitcoin uses)
            return this.Transactions.Where(transaction => transaction.To.Equals(user)).Count() - Transactions.Where(transaction => transaction.From.Equals(user)).Count();
        }
    }
}