using System.Linq;
using System.Reactive.Linq;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;

    public WalletSectionViewModel(WalletAppService walletAppService, IWalletFactory walletFactory,
        IWalletBuilder builder,
        UIServices services,
        IWalletRepository walletUnlocker)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        walletHelper = services.ActiveWallet.CurrentChanged
            .Merge(Observable.Return(services.ActiveWallet.Current).Values())
            .Select(w => new WalletViewModel(w, services, walletUnlocker))
            .ToProperty(this, x => x.Wallet);
        
        RecoverWallet = ReactiveCommand.CreateFromTask(walletFactory.Recover);
        SetDefaultWallet = ReactiveCommand.CreateFromTask(async () =>
        {
            var walletInfos = (await walletAppService.GetWallets()).ToList();
            if (walletInfos.Count == 0 || services.ActiveWallet.Current.HasValue)
            {
                return;
            }
            
            var walletInfo = walletInfos.First();
            await builder.Create(walletInfo.Id)
                .Tap(w => services.ActiveWallet.Current = w.AsMaybe());
        });
        
        IObservable<CombinedReactiveCommand<Unit, Result>> loadCommand = this.WhenAnyValue(x => x.Wallet!.Wallet.Load).WhereNotNull();
        loadCommand.Select(command => command.Select(results => results.Combine().TapError(s => services.NotificationService.Show(s, "Failure"))).Subscribe()).Subscribe();

        IsBusy = SetDefaultWallet.IsExecuting;
    }

    public IObservable<bool> IsBusy { get; set; }

    public ReactiveCommand<Unit,Unit> SetDefaultWallet { get; }

    public ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
}