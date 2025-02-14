using CSharpFunctionalExtensions;

namespace SuppaWallet.Gui.Model;

public interface IWalletProvider
{
    Maybe<IWallet> CurrentWallet { get; set; }
    IObservable<IWallet> CurrentWallets { get; }
}