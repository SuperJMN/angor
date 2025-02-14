using CSharpFunctionalExtensions;

namespace Angor.UI.Model.Wallet;

public interface IWalletProvider
{
    Maybe<IWallet> CurrentWallet { get; set; }
    IObservable<IWallet> CurrentWallets { get; }
}