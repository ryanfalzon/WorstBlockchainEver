using Newtonsoft.Json;

namespace SlightlyBetterBlockchain.Models.ProtocolObjects
{
    public class CountObject
    {
        [JsonProperty("blocks")]
        public int Blocks { get; set; }
    }
}