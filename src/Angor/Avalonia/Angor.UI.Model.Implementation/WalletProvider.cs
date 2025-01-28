using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services.Wallet;

namespace Angor.UI.Model.Implementation;

public class WalletProvider(WalletAppService walletAppService) : IWalletProvider
{
    private Maybe<IWallet> currentWallet;

    public Maybe<IWallet> GetWallet()
    {
        return walletAppService.GetWallets().Select(id => new RuntimeWallet(id, walletAppService)).TryFirst<IWallet>();
    }

    public void SetWallet(IWallet wallet)
    {
        currentWallet = wallet.AsMaybe();
    }
}