using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorstBlockchainEver.Models
{
    public class Transaction
    {
        public ushort Number { get; set; }

        public long Time { get; set; }

        public string From { get; set; }

        public string To { get; set; }
    }
}