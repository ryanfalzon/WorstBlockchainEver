using Newtonsoft.Json;

namespace SlightlyBetterBlockchain.Models.ProtocolObjects
{
    public class BlockObject
    {
        [JsonProperty("block")]
        public Block Block { get; set; }
    }
}