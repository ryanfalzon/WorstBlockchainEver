using Newtonsoft.Json;
using System.Collections.Generic;

namespace SlightlyBetterBlockchain.Models.ProtocolObjects
{
    public class BlockHashesObject
    {
        [JsonProperty("hashes")]
        public List<string> Hashes { get; set; }
    }
}