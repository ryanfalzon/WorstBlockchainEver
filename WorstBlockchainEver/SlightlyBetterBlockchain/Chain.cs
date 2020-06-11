using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SlightlyBetterBlockchain
{
    public class Chain
    {
        public ConcurrentQueue<string> MissingBlocks { get; set; }

        public ConcurrentBag<Block> Temporary { get; set; }

        public List<Block> Blocks { get; set; }

        public Block CurrentBlock { get; set; }

        public Chain()
        {
            Tools.Log("Creating new chain...");

            this.MissingBlocks = new ConcurrentQueue<string>();
            this.Blocks = new List<Block>();
            this.CurrentBlock = null;

            Tools.Log("Chain created successfully!");
        }

        public bool AddBlock(Block block)
        {
            try
            {
                this.Blocks.Add(block);
                this.CurrentBlock = block;

                return true;
            }
            catch(Exception e)
            {
                Tools.Log($"An error occured while adding block!\n{e.Message}");
                return false;
            }
        }

        public bool TryAddBlock(Block block)
        {
            // If can be added to main chain add it, or else add to temporary chain
            var previousBlock = Client.Chain.Blocks.Where(b => b.Hash.Equals(block.HashedContent.PreviousBlockHash)).FirstOrDefault();
            if (previousBlock != null)
            {
                Client.Chain.Blocks.Insert(Client.Chain.Blocks.IndexOf(previousBlock) + 1, block);
                return true;
            }
            else
            {
                Client.Chain.Temporary.Add(block);
                return false;
            }
        }
    }
}