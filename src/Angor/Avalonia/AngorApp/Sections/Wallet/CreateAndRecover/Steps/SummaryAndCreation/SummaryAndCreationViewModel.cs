using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Application;
using Angor.Wallet.Domain;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Helpers;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public partial class SummaryAndCreationViewModel : ReactiveValidationObject, IStep, ISummaryAndCreationViewModel
{
    private readonly IWalletBuilder walletBuilder;
    private readonly UIServices uiServices;
    private readonly Func<BitcoinNetwork> getNetwork;
    private readonly IWalletAppService walletAppService;
    private readonly IWalletUnlockHandler unlockHandler;
    [ObservableAsProperty] private IWallet? wallet;

    public SummaryAndCreationViewModel(IWalletAppService walletAppService, IWalletUnlockHandler unlockHandler, WalletSecurityParameters walletSecurityParameters, IWalletBuilder walletBuilder, UIServices uiServices, Func<BitcoinNetwork> getNetwork)
    {
        this.walletAppService = walletAppService;
        this.unlockHandler = unlockHandler;
        this.walletBuilder = walletBuilder;
        this.uiServices = uiServices;
        this.getNetwork = getNetwork;
        this.Passphrase = walletSecurityParameters.Passphrase;
        this.CreateWallet = ReactiveCommand.CreateFromTask(() => Create(walletSecurityParameters.Seedwords, walletSecurityParameters.EncryptionKey, walletSecurityParameters.EncryptionKey));
        this.walletHelper = CreateWallet.Successes().ToProperty(this, x => x.Wallet);
    }

    private Task<Result<IWallet>> Create(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return walletAppService.ImportWallet("Main", string.Join(" ", seedwords), passphrase, encryptionKey, getNetwork())
            .Bind(walletId => walletBuilder.Create(walletId))
            .Tap(w => unlockHandler.ConfirmUnlock(w.Id, encryptionKey))
            .Tap(w => uiServices.ActiveWallet.Current = w.AsMaybe());
    }

    public string CreateWalletText => IsRecovery ? "Recover Wallet" : "Create Wallet";
    public string CreatingWalletText => IsRecovery ? "Recovering Wallet..." : "Creating Wallet...";

    public string TitleText => IsRecovery ? "You are all set to recover your wallet" : "You are all set to create your wallet";

    public required bool IsRecovery { get; init; }
    public IObservable<bool> IsValid => this.WhenAnyValue<SummaryAndCreationViewModel, IWallet>(x => x.Wallet!).NotNull();
    public IObservable<bool> IsBusy => CreateWallet.IsExecuting;
    public bool AutoAdvance => true;
    public ReactiveCommand<Unit, Result<IWallet>> CreateWallet { get; }
    public Maybe<string> Passphrase { get; }
    public Maybe<string> Title => "Summary";
}