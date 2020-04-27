namespace WorstBlockchainEver.Models
{
    public class Transaction
    {
        public ushort Number { get; set; }

        public long Time { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public bool Approved { get; set; }

        public ushort ApproveTransaction { get; set; }
    }
}