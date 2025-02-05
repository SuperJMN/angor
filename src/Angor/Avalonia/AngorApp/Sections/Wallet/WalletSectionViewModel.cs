using System.Linq;
using System.Reactive.Linq;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWallet.Application;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;

    public WalletSectionViewModel(WalletAppService walletAppService, IWalletFactory walletFactory,
        IWalletProvider walletProvider,
        IWalletBuilder builder,
        UIServices services)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        walletHelper = walletProvider.CurrentWallets
            .Select(w => new WalletViewModel(w, services))
            .ToProperty(this, x => x.Wallet);
        
        RecoverWallet = ReactiveCommand.CreateFromTask(walletFactory.Recover);
        SetDefaultWallet = ReactiveCommand.CreateFromTask(async () =>
        {
            var walletInfos = await walletAppService.GetWallets();
            var walletInfo = walletInfos.First();
            await builder.Create(walletInfo.Id)
                .Tap(w => walletProvider.CurrentWallet = w.AsMaybe());
        }, Observable.FromAsync(walletAppService.GetWallets).Select(x => x.Any()));
        
        SetDefaultWallet.Execute().Subscribe();

        IObservable<CombinedReactiveCommand<Unit, Result>> loadCommand = this.WhenAnyValue(x => x.Wallet!.Wallet.Load).WhereNotNull();
        loadCommand.Select(command => command.Select(results => results.Combine().TapError(s => services.NotificationService.Show(s, "Failure"))).Subscribe()).Subscribe();
    }

    public ReactiveCommand<Unit,Unit> SetDefaultWallet { get; }

    public ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
}