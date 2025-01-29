using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Core;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWallet.Application.Services.Wallet;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;
    
    public WalletSectionViewModel(IWalletFactory walletFactory, WalletAppService walletAppService, IWalletProvider walletProvider, UIServices services)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        var wallets = CreateWallet.Values().Successes();
        wallets.Do(walletProvider.SetWallet).Subscribe();

        
        LoadWallet = ReactiveCommand.CreateFromTask(() =>
        {
            return walletAppService.ImportWallet(SampleData.WalletDescriptor())
                .Map(walletId => new RuntimeWallet(walletId, walletAppService));
        }, this.WhenAnyValue(x => x.Wallet).NotNull());
        
        var initial = LoadWallet.Successes();
        walletHelper = wallets.Merge(initial).Select(w => new WalletViewModel(w, services)).ToProperty<WalletSectionViewModel, IWalletViewModel>(this, x => x.Wallet);
        RecoverWallet = ReactiveCommand.CreateFromTask(walletFactory.Recover);

        LoadWallet.Execute().Subscribe();
    }

    public ReactiveCommand<Unit,Result<RuntimeWallet>> LoadWallet { get; set; }

    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> CreateWallet { get; }
    public ReactiveCommand<Unit, Maybe<Result<IWallet>>> RecoverWallet { get; }
}