using Newtonsoft.Json;

namespace SlightlyBetterBlockchain.Models.ProtocolObjects
{
    public class RequestBlockObject
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}