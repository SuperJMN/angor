using CSharpFunctionalExtensions;

namespace SuppaWallet.Domain;

public interface IWalletRepository
{
    Task<Result<IEnumerable<(WalletId Id, string Name)>>> ListWallets();
    Task<Result<Wallet>> Get(WalletId id);
}