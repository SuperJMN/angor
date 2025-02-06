using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

public interface IWalletImporter
{
    Task<Result<Wallet>> ImportWallet(string name, string seedwords, Maybe<string> passphrase, string encryptionKey, BitcoinNetwork network);
}