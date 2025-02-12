using System.Threading.Tasks;
using AngorApp.Sections.Wallet.CreateAndRecover;
using CSharpFunctionalExtensions;
using SuppaWallet.Gui.Model;

namespace AngorApp.UI.Services;

public class WalletFactory : IWalletFactory
{
    private readonly UIServices uiServices;
    private readonly IWalletBuilder walletBuilder;

    public WalletFactory(IWalletBuilder walletBuilder, UIServices uiServices)
    {
        this.walletBuilder = walletBuilder;
        this.uiServices = uiServices;
    }

    public Task<Maybe<IWallet>> Recover()
    {
        throw new NotImplementedException();
        //return new Recover(uiServices, walletBuilder).Start();
    }

    public Task<Maybe<IWallet>> Create()
    {
        throw new NotImplementedException();
        //return new Create(uiServices, walletBuilder).Start();
    }
}