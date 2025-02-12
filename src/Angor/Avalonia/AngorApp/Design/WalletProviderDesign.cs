using CSharpFunctionalExtensions;
using SuppaWallet.Gui.Model;

namespace AngorApp.Design;

public class WalletProviderDesign : IWalletProvider
{
    public Maybe<IWallet> CurrentWallet { get; set; }
    public IObservable<IWallet> CurrentWallets { get; }
}