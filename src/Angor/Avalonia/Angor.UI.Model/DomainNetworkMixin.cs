namespace Angor.UI.Model;

public static class DomainNetworkMixin
{
    public static RefinedSuppaWallet.Domain.BitcoinNetwork ToDomain(this Model.BitcoinNetwork network)
    {
        return network switch
        {
            BitcoinNetwork.Mainnet => RefinedSuppaWallet.Domain.BitcoinNetwork.Mainnet,
            BitcoinNetwork.Testnet => RefinedSuppaWallet.Domain.BitcoinNetwork.Testnet,
            _ => throw new ArgumentOutOfRangeException(nameof(network), network, null)
        };
    }
}