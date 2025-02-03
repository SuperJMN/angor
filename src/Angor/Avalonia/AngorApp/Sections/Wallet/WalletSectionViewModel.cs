using System.Reactive.Linq;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;

    public WalletSectionViewModel(IWalletFactory walletFactory, IWalletProvider walletProvider, UIServices services, WalletAppService walletAppService, IWalletUnlocker walletUnlocker)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        var wallets = CreateWallet.Values().Successes();
        wallets.Do(w => walletProvider.SetWallet(w.Id)).Subscribe();

        LoadWallet = ReactiveCommand.CreateFromTask(() => walletProvider.GetWalletId()
                .Map(id => (IWallet)new RuntimeWallet(id, walletAppService, walletUnlocker)),
            this.WhenAnyValue(x => x.Wallet).NotNull());

        walletHelper = wallets.Merge(LoadWallet.Values()).Select(w => new WalletViewModel(w, services)).ToProperty<WalletSectionViewModel, IWalletViewModel>(this, x => x.Wallet);
        RecoverWallet = ReactiveCommand.CreateFromTask(walletFactory.Recover);

        LoadWallet.Execute().Subscribe();
    }

    public ReactiveCommand<Unit, Maybe<IWallet>> LoadWallet { get; }
    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> RecoverWallet { get; }
}