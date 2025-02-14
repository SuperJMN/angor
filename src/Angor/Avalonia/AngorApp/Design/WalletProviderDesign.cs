using Angor.UI.Model.Wallet;
using CSharpFunctionalExtensions;

namespace AngorApp.Design;

public class WalletProviderDesign : IWalletProvider
{
    public Maybe<IWallet> CurrentWallet { get; set; }
    public IObservable<IWallet> CurrentWallets { get; }
}