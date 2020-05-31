using SlightlyBetterBlockchain.Helper;
using SlightlyBetterBlockchain.Models;
using System;
using System.Collections.Generic;

namespace SlightlyBetterBlockchain
{
    public class Chain
    {
        public List<Block> Blocks { get; set; }

        public Block CurrentBlock { get; set; }

        public Chain()
        {
            Tools.Log("Creating new chain...");

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
    }
}