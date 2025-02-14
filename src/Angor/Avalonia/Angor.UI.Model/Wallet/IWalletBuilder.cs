using CSharpFunctionalExtensions;
using SuppaWallet.Domain;

namespace SuppaWallet.Gui.Model;

public interface IWalletBuilder
{
    Task<Result<IWallet>> Create(WalletId walletId);
}