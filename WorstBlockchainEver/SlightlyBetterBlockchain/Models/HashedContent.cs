using Newtonsoft.Json;
using System.Collections.Generic;

namespace SlightlyBetterBlockchain.Models
{
    public class HashedContent
    {
        public string PreviousBlockHash { get; set; }

        public int Nonce { get; set; }

        public long Timestamp { get; set; }

        public List<Transaction> Transactions { get; set; }
    }
}