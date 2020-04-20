using System.Net;

namespace WorstBlockchainEver.Models
{
    public class Node
    {
        public int Id { get; set; }

        public IPAddress IPAddress { get; set; }

        public int Port { get; set; }
    }
}