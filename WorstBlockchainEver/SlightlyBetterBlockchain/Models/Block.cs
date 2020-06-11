using Newtonsoft.Json;
using SlightlyBetterBlockchain.Helper;

namespace SlightlyBetterBlockchain.Models
{
    public class Block
    {
        [JsonProperty("hashedContent")]
        public HashedContent HashedContent { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        private void IncreaseNonce()
        {
            this.HashedContent.Nonce++;
        }

        public void CalculateHash(string difficulty)
        {
            string hash;
            bool found;
            do
            {
                hash = Tools.ToHex(Tools.GetSha256Hash(Tools.EncodeUtf8(JsonConvert.SerializeObject(this.HashedContent))));

                found = FoundHash(hash, difficulty);
                if (!found)
                {
                    this.IncreaseNonce();
                }

            } while (!found);

            this.Hash = hash;
        }

        private bool FoundHash(string hash, string difficulty)
        {
            return hash.StartsWith(difficulty);
        }

        public new string ToString => HashedContent.ToString();
    }
}