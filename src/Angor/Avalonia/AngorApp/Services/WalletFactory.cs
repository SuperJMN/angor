using System.Threading.Tasks;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.CreateAndRecover;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;

namespace AngorApp.Services;

public class WalletFactory : IWalletFactory
{
    private readonly UIServices uiServices;
    private readonly IWalletAppService walletAppService;
    private readonly IWalletUnlockHandler walletUnlockHandler;
    private readonly IWalletBuilder walletBuilder;

    public WalletFactory(IWalletBuilder walletBuilder, UIServices uiServices, IWalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler)
    {
        this.walletBuilder = walletBuilder;
        this.uiServices = uiServices;
        this.walletAppService = walletAppService;
        this.walletUnlockHandler = walletUnlockHandler;
    }

    public Task<Maybe<IWallet>> Recover()
    {
        return new Recover(uiServices, walletBuilder, walletAppService, walletUnlockHandler).Start();
    }

    public Task<Maybe<IWallet>> Create()
    {
        return new Create(uiServices, walletBuilder, walletAppService, walletUnlockHandler).Start();
    }
}