using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorstBlockchainEver.Models
{
    public enum States
    {
        Syncing,

        Synchronized,

        NotSynchronized,

        AwaitingTransactions,

        SendingTransactions
    }
}
