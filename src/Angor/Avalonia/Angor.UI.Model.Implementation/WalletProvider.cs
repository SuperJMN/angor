using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class WalletProvider(WalletAppService walletAppService) : IWalletProvider
{
    public async Task<Maybe<WalletId>> GetWalletId()
    {
        return (await walletAppService.GetWallets()).TryFirst().Select(s => s.Id);
    }

    public void SetWallet(WalletId wallet)
    {
    }
}