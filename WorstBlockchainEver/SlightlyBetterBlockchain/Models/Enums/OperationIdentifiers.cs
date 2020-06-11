using SlightlyBetterBlockchain.Helper;

namespace SlightlyBetterBlockchain.Models.Enums
{
    public static class OperationIdentifiers
    {
        public static byte GetCount = Tools.EncodeChar('a');

        public static byte Count = Tools.EncodeChar('c');

        public static byte GetBlockHashes = Tools.EncodeChar('b');

        public static byte BlockHashes = Tools.EncodeChar('h');

        public static byte RequestBlock = Tools.EncodeChar('r');

        public static byte Block = Tools.EncodeChar('x');

        public static byte NewBlock = Tools.EncodeChar('z');
    }
}