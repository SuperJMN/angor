using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services;

namespace Angor.UI.Model.Implementation;

public class WalletProvider(WalletAppService walletAppService) : IWalletProvider
{
    public async Task<Maybe<IWallet>> GetWallet()
    {
        return (await walletAppService.GetWallets()).Select(tuple => new RuntimeWallet(tuple.Id, walletAppService)).TryFirst<IWallet>();
    }

    public void SetWallet(IWallet wallet)
    {
    }
}