using System.Threading.Tasks;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.CreateAndRecover;
using CSharpFunctionalExtensions;

namespace AngorApp.Services;

public class WalletFactory : IWalletFactory
{
    private readonly UIServices uiServices;
    private readonly IWalletImporter walletImporter;
    private readonly IWalletProvider walletProvider;
    private readonly IWalletBuilder walletBuilder;

    public WalletFactory(IWalletBuilder walletBuilder, UIServices uiServices, IWalletImporter walletImporter, IWalletProvider walletProvider)
    {
        this.walletBuilder = walletBuilder;
        this.uiServices = uiServices;
        this.walletImporter = walletImporter;
        this.walletProvider = walletProvider;
    }

    public Task<Maybe<IWallet>> Recover()
    {
        return new Recover(uiServices, walletBuilder, walletImporter, walletProvider).Start();
    }

    public Task<Maybe<IWallet>> Create()
    {
        return new Create(uiServices, walletBuilder, walletImporter, walletProvider).Start();
    }
}