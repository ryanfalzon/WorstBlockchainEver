using Newtonsoft.Json;
using SlightlyBetterBlockchain.Helper;

namespace SlightlyBetterBlockchain.Models
{
    public class Transaction
    {
        public string From { get; set; }

        public string To { get; set; }

        public string Hash { get; set; }

        public void CalculateHash()
        {
            this.Hash = Tools.ToHex(Tools.GetSha256Hash(Tools.EncodeUtf8(JsonConvert.SerializeObject(this))));
        }
    }
}