using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation.Tests;

public static class SampleData
{
    public static WalletDescriptor WalletDescriptor()
    {
        var segwitXPub = XPub.Create("tpubDCdeHNHx9LPdHrtyFmC4wSDJpe856zWULw71cJzyfTcX8BuAA2VVrd2VbJG4gfjdnJ7oVsVGFc9VxwteBizEstLCtSgqkzKUyHepywymwTi",
            ScriptType.SegWit,
            DerivationPath.Create(84, 1, 0));
        var taprootXPub = XPub.Create("tpubDDXiQKQr38AGPXZ1nVktkTYdJVMj3E7Xioqg2ED5xUN4PtNUZuM5AcK7s87ep2mMGSmFee36tCRiAyfXqZJhuvN1xB2JdixBrFM5hzBCr4Y",
            ScriptType.Taproot,
            DerivationPath.Create(86, 1, 0));

        var xPubCollection = XPubCollection.Create(segwitXPub, taprootXPub);

        var walletDescriptor = RefinedSuppaWallet.Domain.WalletDescriptor.Create("b750f1ea", RefinedSuppaWallet.Domain.BitcoinNetwork.Testnet, xPubCollection);
        return walletDescriptor;
    }

    public static Wallet Wallet()
    {
        return new Wallet(WalletId.New(), WalletDescriptor());
    }
}