using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public interface IEncryptionKeyProvider
{
    Task<Result<string>> GetEncryptionKey(WalletId walletId);
}