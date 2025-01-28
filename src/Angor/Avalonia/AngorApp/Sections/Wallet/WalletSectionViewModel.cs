using System.Reactive.Linq;
using System.Windows.Input;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;
    
    public WalletSectionViewModel(IWalletFactory walletFactory, IWalletProvider walletProvider, UIServices services)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        var wallets = CreateWallet.Values().Successes();
        wallets.Do(walletProvider.SetWallet).Subscribe();
        var initial = Observable.Return(walletProvider.GetWallet()).Values();
        walletHelper = wallets.Merge(initial).Select(w => new WalletViewModel(w, services)).ToProperty<WalletSectionViewModel, IWalletViewModel>(this, x => x.Wallet);
        RecoverWallet = ReactiveCommand.CreateFromTask(walletFactory.Recover);
    }

    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> RecoverWallet { get; }
}