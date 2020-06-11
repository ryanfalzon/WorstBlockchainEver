using System.Net;

namespace SlightlyBetterBlockchain.Models
{
    public class Node
    {
        public int Id { get; set; }

        public IPAddress IPAddress { get; set; }

        public int Port { get; set; }
    }
}