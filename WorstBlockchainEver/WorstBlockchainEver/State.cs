using System.Collections.Generic;
using System.Linq;
using WorstBlockchainEver.Helper;
using WorstBlockchainEver.Models;

namespace WorstBlockchainEver
{
    public class State
    {
        public List<Transaction> Transactions{ get; set; }

        public int NetworkTransactionCount { get; set; }

        public void Synchronize()
        {
            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.HighestTransaction));
        }

        public void GetTransaction(ushort number)
        {
            Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.GetTransaction, new string[] { number.ToString() }));
        }

        public void AddLocalTransaction(Transaction transaction)
        {
            if(CheckBalance(transaction.From) >= 1 || transaction.From.Equals("0x"))
            {
                this.Transactions.Add(transaction);
                Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NewTransaction, new string[]
                {
                    transaction.Number.ToString(),
                    transaction.Time.ToString(),
                    transaction.From,
                    transaction.To
                }));
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
            return Transactions.Where(transaction => transaction.To.Equals(user)).Count() - Transactions.Where(transaction => transaction.From.Equals(user)).Count();
        }
    }
}