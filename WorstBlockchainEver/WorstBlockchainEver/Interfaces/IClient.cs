namespace WorstBlockchainEver.Interfaces
{
    public interface IClient
    {
        void InitPeers();

        void LoadSeedNodes();

        void DiscoverOtherNodes();

        void BroadcastMessage(string message);

        void ProcessIncomingMessages();
    }
}