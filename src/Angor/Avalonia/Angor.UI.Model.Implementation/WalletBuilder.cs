using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class WalletBuilder : IWalletBuilder
{
    private readonly WalletAppService walletAppService;

    public WalletBuilder(WalletAppService walletAppService)
    {
        this.walletAppService = walletAppService;
    }
    
    public async Task<Result<IWallet>> Create(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return new RuntimeWallet(WalletId.New(), walletAppService);
    }
}