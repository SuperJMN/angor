using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class WalletBuilder : IWalletBuilder
{
    private readonly WalletAppService walletAppService;
    private readonly IWalletUnlockHandler walletUnlockHandler;

    public WalletBuilder(WalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler)
    {
        this.walletAppService = walletAppService;
        this.walletUnlockHandler = walletUnlockHandler;
    }

    public async Task<Result<IWallet>> Create(WalletId id)
    {
        return new DynamicWallet(id, walletAppService, walletUnlockHandler);
    }
}