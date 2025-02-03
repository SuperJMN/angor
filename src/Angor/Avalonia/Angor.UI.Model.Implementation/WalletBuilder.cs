using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class WalletBuilder : IWalletBuilder
{
    private readonly WalletAppService walletAppService;
    private readonly IWalletUnlocker walletUnlocker;

    public WalletBuilder(WalletAppService walletAppService, IWalletUnlocker walletUnlocker)
    {
        this.walletAppService = walletAppService;
        this.walletUnlocker = walletUnlocker;
    }

    public async Task<Result<IWallet>> Create(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return new DynamicWallet(WalletId.New(), walletAppService, walletUnlocker);
    }
}