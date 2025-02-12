using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using SuppaWallet.Application.Interfaces;
using SuppaWallet.Gui.Model;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;

    public WalletSectionViewModel(IWalletAppService walletAppService, IWalletBuilder walletBuilder, IWalletFactory walletFactory, UIServices uiServices)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(() => walletFactory.Create());
        RecoverWallet = ReactiveCommand.CreateFromTask(() => walletFactory.Recover());

        walletHelper = uiServices.ActiveWallet.CurrentChanged
            .Merge(Observable.Return(uiServices.ActiveWallet.Current).Values())
            .Select(w => new WalletViewModel(w, uiServices))
            .ToProperty(this, x => x.Wallet);
        
        SetDefaultWallet = ReactiveCommand.CreateFromTask(() => DoSetDefaultWallet(walletAppService, walletBuilder, uiServices));
    }

    private static async Task DoSetDefaultWallet(IWalletAppService walletAppService, IWalletBuilder walletBuilder, UIServices uiServices)
    {
        var result = await walletAppService.GetWallets().Map(tuples => tuples.ToList());
        if (result.IsFailure)
        {
            return;
        }
            
        var walletInfos = result.Value;
        if (walletInfos.Count == 0 || uiServices.ActiveWallet.Current.HasValue)
        {
            return;
        }

        var walletInfo = walletInfos.First();
        await walletBuilder.Create(walletInfo.Id)
            .Tap(w => uiServices.ActiveWallet.Current = w.AsMaybe());
    }

    public ReactiveCommand<Unit,Unit> SetDefaultWallet { get; }
    public ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
}