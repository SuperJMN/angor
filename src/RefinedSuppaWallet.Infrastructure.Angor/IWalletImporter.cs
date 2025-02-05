using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

public interface IWalletImporter
{
    Task<Result<Wallet>> ImportWallet(string name, string seed, string encryptionKey, BitcoinNetwork network, bool requiresPassphrase = false);
}