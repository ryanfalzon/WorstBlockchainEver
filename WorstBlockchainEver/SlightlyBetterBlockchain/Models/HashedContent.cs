using Newtonsoft.Json;
using System.Collections.Generic;

namespace SlightlyBetterBlockchain.Models
{
    public class HashedContent
    {
        [JsonProperty("previousBlockHash")]
        public string PreviousBlockHash { get; set; }

        [JsonProperty("nonce")]
        public int Nonce { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("transactions")]
        public List<Transaction> Transactions { get; set; }
    }
}