using Newtonsoft.Json;

namespace SlightlyBetterBlockchain.Models.ProtocolObjects
{
    public class NewBlockObject
    {
        [JsonProperty("block")]
        public Block Block { get; set; }
    }
}