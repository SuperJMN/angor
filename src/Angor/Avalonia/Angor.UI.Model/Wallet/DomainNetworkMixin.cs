namespace SuppaWallet.Gui.Model;

public static class DomainNetworkMixin
{
    public static Domain.BitcoinNetwork ToDomain(this BitcoinNetwork network)
    {
        return network switch
        {
            BitcoinNetwork.Mainnet => Domain.BitcoinNetwork.Mainnet,
            BitcoinNetwork.Testnet => Domain.BitcoinNetwork.Testnet,
            _ => throw new ArgumentOutOfRangeException(nameof(network), network, null)
        };
    }
}