using AngorApp.Model;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public class WalletProviderDesign : IWalletProvider
{
    private Maybe<IWallet> maybeWallet = Maybe<IWallet>.None;

    public Maybe<IWallet> GetWallet()
    {
        return maybeWallet;
    }

    public void SetWallet(IWallet wallet)
    {
        this.maybeWallet = wallet.AsMaybe();
    }
}