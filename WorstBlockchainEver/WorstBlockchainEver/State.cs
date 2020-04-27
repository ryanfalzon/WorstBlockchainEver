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

        public Dictionary<int, int> TransactionsNotApproved { get; set; }

        public BalanceDictionary<string, long> Balances { get; set; }

        public int NetworkTransactionCount { get; set; }

        public int SyncCounter { get; set; }

        public State()
        {
            Transactions = new List<Transaction>();
            TransactionsNotApproved = new Dictionary<int, int>();
            Balances = new BalanceDictionary<string, long>(0);
            SyncCounter = 0;
        }

        public bool IsSynced()
        {
            return Transactions.Count == NetworkTransactionCount;
        }

        public void Synchronize()
        {
            // Get all transactions that are not present locally by asking other nodes for them
            for (int i = Transactions.Count; i <= NetworkTransactionCount; i++)
            {
                // We do not need transaction with number 0
                if (i != 0)
                {
                    GetTransaction((ushort)i);
                }
            }

            // Wait until all transaction have been received
            while (Transactions.Count != NetworkTransactionCount)
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
            while (CurrentState != States.Synchronized)
            {
                Thread.Sleep(1000);
            }

            // Set current state to sending transaction
            CurrentState = States.SendingTransaction;

            // Check if user has sufficient balance or the user is the 0x address
            if (CheckBalance(transaction.From) >= 1 || transaction.From.Equals("00"))
            {
                Tools.Log($"Creating new transaction with number {transaction.Number}...");

                // Add transaction to local chain and broadcast new transaction to network
                Transactions.Add(transaction);
                Client.Peers.BroadcastMessage(Protocol.CreateMessage(Commands.NewTransaction, new string[]
                {
                        transaction.Number.ToString(),
                        transaction.Time.ToString(),
                        transaction.From,
                        transaction.To,
                        transaction.Approved.ToString(),
                        transaction.ApproveTransaction.ToString()
                }));

                if (transaction.Approved) UpdateBalance(transaction);

                Tools.Log($"Transaction with number {transaction.Number} created successfully and was broadcasted to network");
            }
            else
            {
                Tools.Log($"Could not create transaction as user {transaction.From} has insufficient balance!");
            }

            // Set current state to synchronized
            CurrentState = States.Synchronized;
        }

        public void AddNetworkTransaction(Transaction transaction)
        {
            Tools.Log($"New transaction with number {transaction.Number} received...");

            // Check if transaction with same number exists in the local transaction chain
            var transactionExists = Transactions.Exists(t => t.Number == transaction.Number);
            if (transactionExists)
            {
                Tools.Log($"Transaction with number {transaction.Number} already exists...");
                var existingTransaction = Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault();

                // Check if the newly received transaction is older than the one present in local transaction chain
                if (transaction.Time <= existingTransaction.Time)
                {
                    // Replace the newly received transaction with the one already present in local transaction chain
                    Tools.Log($"Transaction received is older...");
                    ReplaceTransaction(transaction);

                    // Add transaction to list of not approved transactions
                    if(!transaction.Approved && transaction.To.Equals(Client.User))
                    {
                        TransactionsNotApproved.Add(transaction.Number, Transactions.IndexOf(Transactions.Where(t => t.Number == transaction.Number).First()));
                    }
                }
                else
                {
                    // Discard newly received transaction as it is newer
                    Tools.Log($"Transaction received will be discarded as it is older...");
                }
            }
            else
            {
                // Add transaction to local transaction chain
                Transactions.Add(transaction);
                UpdateBalance(transaction);

                // Add transaction to list of not approved transactions
                if (!transaction.Approved && transaction.To.Equals(Client.User))
                {
                    TransactionsNotApproved.Add(transaction.Number, Transactions.IndexOf(Transactions.Where(t => t.Number == transaction.Number).First()));
                }

                Tools.Log($"Transaction with number {transaction.Number} added successfully!");
            }
        }

        public void ReplaceTransaction(Transaction transaction)
        {
            Tools.Log($"Replacing received transaction with number {transaction.Number}...");

            Transactions.Insert(Transactions.IndexOf(Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault()), transaction);
            UpdateBalance(Transactions.Where(t => t.Number == transaction.Number).FirstOrDefault(), true);
            UpdateBalance(transaction);

            Tools.Log($"Transaction with number {transaction.Number} replaced successfully!");
        }

        public long CheckBalance(string user)
        {
            return !user.Equals("00") ? Balances.Get(user) : 0;
        }

        private void UpdateBalance(Transaction transaction, bool reverse = false)
        {
            var multiplier = reverse ? -1 : 1;

            // If the transaction is one of the initial award transaction
            // Only the to needs to be affected this time
            if(transaction.Approved && transaction.ApproveTransaction == 0 && transaction.From.Equals("00"))
            {
                Balances.Modify(transaction.To, Balances.Get(transaction.To) + (multiplier * 1));
            }
            // If the transaction is not approved and is awaiting approval
            // Only if we are reverting the transaction enter here as if it is not approved it will not affect the balance
            else if(!transaction.Approved && reverse)
            {
                var approvedTransactionExists = Transactions.Exists(t => t.ApproveTransaction == transaction.Number);
                if (approvedTransactionExists)
                {
                    var approvedTransaction = Transactions.Where(t => t.ApproveTransaction == transaction.Number).FirstOrDefault();
                    Balances.Modify(approvedTransaction.To, Balances.Get(approvedTransaction.To) + (multiplier * 1));
                    Balances.Modify(approvedTransaction.From, Balances.Get(approvedTransaction.From) - (multiplier * 1));
                }
            }
            // If the transaction is approved but required approval
            // Or the transaction has always been approved
            else if(transaction.Approved && transaction.ApproveTransaction > 0 && transaction.From.Equals("00"))
            {
                var approveTransaction = Transactions.Where(t => t.Number == transaction.ApproveTransaction).First();

                Balances.Modify(transaction.To, Balances.Get(transaction.To) + (multiplier * 1));
                Balances.Modify(approveTransaction.From, Balances.Get(approveTransaction.From) - (multiplier * 1));
            }
            // If the transaction has always been approved
            else if(transaction.Approved && transaction.ApproveTransaction == 0 && !transaction.From.Equals("00"))
            {
                Balances.Modify(transaction.To, Balances.Get(transaction.To) + (multiplier * 1));
                Balances.Modify(transaction.From, Balances.Get(transaction.From) - (multiplier * 1));
            }
        }
    }
}