namespace WorstBlockchainEver.Interfaces
{
    public interface IPeers
    {
        void InitPeers();

        void LoadSeedNodes();

        void DiscoverOtherNodes();

        void BroadcastMessage(byte[] message);

        void ProcessIncomingMessages();
    }
}