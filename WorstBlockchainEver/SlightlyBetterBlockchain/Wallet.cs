using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SlightlyBetterBlockchain.Helper;
using System.Linq;

namespace SlightlyBetterBlockchain
{
    public class Wallet
    {
        public ECPublicKeyParameters PublicKey { get; set; }

        public ECPrivateKeyParameters PrivateKey { get; set; }

        public Wallet()
        {
            Tools.Log("Creating wallet...");

            var curve = ECNamedCurveTable.GetByName("secp256k1");
            var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

            var secureRandom = new SecureRandom();
            var keyParams = new ECKeyGenerationParameters(domainParams, secureRandom);

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(keyParams);
            var keyPair = generator.GenerateKeyPair();

            this.PrivateKey = keyPair.Private as ECPrivateKeyParameters;
            this.PublicKey = keyPair.Public as ECPublicKeyParameters;

            Tools.Log("Wallet created successfully!");
            Tools.Log($"Public Key - {GetPublicKey()}");
            Tools.Log($"Private Key - {Tools.ToHex(PrivateKey.D.ToByteArrayUnsigned())}");
        }

        public string GetPublicKey()
        {
            return Tools.ToHex(PublicKey.Q.GetEncoded(true));
        }

        public int CheckBalance()
        {
            int balance = 0;
            foreach (var block in Client.Chain.Blocks)
            {
                foreach (var transaction in block.HashedContent.Transactions)
                {
                    if (transaction.From == GetPublicKey())
                    {
                        balance--;
                    }
                    if (transaction.To == GetPublicKey())
                    {
                        balance++;
                    }
                }
            }
            return balance - Client.Miner.MiningPool.Where(transaction => transaction.From.Equals(GetPublicKey())).Count();
        }
    }
}