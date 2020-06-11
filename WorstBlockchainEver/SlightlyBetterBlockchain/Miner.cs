﻿using Newtonsoft.Json;
using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using SlightlyBetterBlockchain.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SlightlyBetterBlockchain
{
    public class Miner
    {
        public int Difficulty { get; set; }

        public string DifficultyString { get; set; }

        public int BlockSize { get; set; }

        public List<Transaction> MiningPool { get; set; }

        public Miner()
        {
            this.Difficulty = Properties.Settings.Default.Difficulty;

            for (int i = 0; i < this.Difficulty; i++)
            {
                this.DifficultyString += '0';
            }

            this.MiningPool = new List<Transaction>();
        }

        public void Run()
        {
            while (true)
            {
                StateAwaiter.Await(States.Mining);

                // Construct block
                Block block = new Block()
                {
                    HashedContent = new HashedContent()
                    {
                        PreviousBlockHash = Client.Chain.CurrentBlock != null ? Client.Chain.CurrentBlock.Hash : string.Empty,
                        Timestamp = Tools.GetUnixTimestamp(DateTime.Now),
                        Nonce = 0
                    }
                };

                // Fill up block with transactions
                List<Transaction> transactions = new List<Transaction>();
                if(this.MiningPool.Count > 0)
                {
                    int i = 0;
                    while((transactions.Count <= this.BlockSize) && (i < this.MiningPool.Count))
                    {
                        transactions.Add(this.MiningPool[i]);
                        i++;
                    }
                }
                block.HashedContent.Transactions = transactions;

                // Add coinbase transaction
                var coinbaseTransaction = new Transaction()
                {
                    To = Client.Wallet.GetPublicKey(),
                    From = "00"
                };
                coinbaseTransaction.CalculateHash();
                block.HashedContent.Transactions.Insert(0, coinbaseTransaction);

                // Find the hash of the block
                block.CalculateHash(this.DifficultyString);
                Tools.Log($"Block Found [{block.Hash}]");
                Tools.Log($"{JsonConvert.SerializeObject(block)}\n");

                // Add new block to chain
                Client.Chain.AddBlock(block);
                ClearMiningPool(transactions);

                Thread.Sleep(Properties.Settings.Default.MiningDelay);
            }
        }

        public void ClearMiningPool(List<Transaction> transactions)
        {
            this.MiningPool = this.MiningPool.Where(mp => !transactions.Any(t => t.Hash.Equals(mp.Hash))).ToList();
        }
    }
}