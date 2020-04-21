namespace WorstBlockchainEver.Models
{
    public enum States
    {
        InitializingSyncProcess,    // The node will go in this state when a message for the highest transaction is broadcasted

        Syncing,                    // The node will be in this state until all seed nodes respond back with the highest transaction

        Synchronized,               // The node will be in this state if the local transaction count matches that of the network

        NotSynchronized,            // The node will be in this state if the local transaction count does not match that of the network

        AwaitingTransactions,       // The node will be in this state until the local transaction count matches that of the network

        SendingTransaction          // The node will be in this state while it is sending a new transaction to the network

        // Possible state transitions:
        // Synchronized => InitializingSyncProcess => Syncing => NotSynchronized => AwaitingTransactions => Synchronized
        // Synchronized => InitializingSyncProcess => Syncing => Synchronized
        // Synchronized => SendingTransaction => Synchronized
        // Synchronized => AwaitingTransactions => Synchronized
    }
}