using Angor.Wallet.Domain;
using CSharpFunctionalExtensions;

namespace Angor.UI.Model.Wallet;

public interface IWalletBuilder
{
    Task<Result<IWallet>> Create(WalletId walletId);
}