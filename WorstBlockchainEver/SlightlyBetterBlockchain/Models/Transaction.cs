using Newtonsoft.Json;
using SlightlyBetterBlockchain.Helper;

namespace SlightlyBetterBlockchain.Models
{
    public class Transaction
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        public void CalculateHash()
        {
            this.Hash = Tools.ToHex(Tools.GetSha256Hash(Tools.EncodeUtf8(JsonConvert.SerializeObject(this))));
        }
    }
}