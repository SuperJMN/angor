using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Core;
using AngorApp.Sections.Wallet.Operate;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using NBitcoin;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWallet.Application.Services;
using RefinedSuppaWallet.Domain;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;

namespace AngorApp.Sections.Wallet;

public partial class WalletSectionViewModel : ReactiveObject, IWalletSectionViewModel
{
    [ObservableAsProperty] private IWalletViewModel? wallet;
    
    public WalletSectionViewModel(IWalletFactory walletFactory, WalletAppService walletAppService, IWalletProvider walletProvider, UIServices services, Func<Dictionary<WalletId, (Network, ExtKey)>> dict)
    {
        CreateWallet = ReactiveCommand.CreateFromTask(walletFactory.Create);
        var wallets = CreateWallet.Values().Successes();
        wallets.Do(walletProvider.SetWallet).Subscribe();

        
        var valueTuple = (Network.TestNet, ExtKey.Parse("tprv8ZgxMBicQKsPd3bePirSewfCg7PQ9KaJ1ztgecjDodoit4yt8zns8AMUQhUFVJNLZgaW9AKKnTKHoLNMuqwBPWxucTW1Vh9F59HC2H9Fro3", Network.TestNet));

        LoadWallet = ReactiveCommand.CreateFromTask(() =>
        {
            return walletAppService.ImportWallet(SampleData.WalletDescriptor(), "Wallet", "tprv8ZgxMBicQKsPd3bePirSewfCg7PQ9KaJ1ztgecjDodoit4yt8zns8AMUQhUFVJNLZgaW9AKKnTKHoLNMuqwBPWxucTW1Vh9F59HC2H9Fro3")
                .Tap(id => dict().Add(id, valueTuple))
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