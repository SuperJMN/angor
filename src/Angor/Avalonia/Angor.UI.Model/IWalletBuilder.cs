using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model;

public interface IWalletBuilder
{
    Task<Result<IWallet>> Create(WalletId walletId);
}