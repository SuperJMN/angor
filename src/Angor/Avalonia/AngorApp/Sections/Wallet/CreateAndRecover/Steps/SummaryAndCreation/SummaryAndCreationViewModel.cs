using System.Threading.Tasks;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Helpers;
using SuppaWallet.Application.Interfaces;
using SuppaWallet.Gui.Model;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;
using SeedWords = Angor.UI.Model.SeedWords;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public partial class SummaryAndCreationViewModel : ReactiveValidationObject, IStep, ISummaryAndCreationViewModel
{
    private readonly IWalletImporter walletImporter;
    private readonly IWalletBuilder walletBuilder;
    private readonly IWalletUnlockHandler unlockHandler;
    private readonly UIServices uiServices;
    private readonly Func<BitcoinNetwork> getNetwork;
    [ObservableAsProperty] private IWallet? wallet;

    public SummaryAndCreationViewModel(IWalletImporter walletImporter, IWalletUnlockHandler unlockHandler, Maybe<string> passphrase, SeedWords seedwords, string encryptionKey, IWalletBuilder walletBuilder,
            Action<Result<IWallet>> creationResult)
    {
        this.walletImporter = walletImporter;
        this.unlockHandler = unlockHandler;
        this.walletBuilder = walletBuilder;
        Passphrase = passphrase;
        CreateWallet = ReactiveCommand.CreateFromTask(() => Create(seedwords, encryptionKey, encryptionKey));
        walletHelper = CreateWallet.Successes().ToProperty(this, x => x.Wallet);
    }

    private Task<Result<IWallet>> Create(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return walletImporter.ImportWallet("Main", string.Join(" ", seedwords), passphrase, encryptionKey, getNetwork().ToDomain())
            .Bind(w => walletBuilder.Create(w.Id))
            .Tap(w => unlockHandler.ConfirmUnlock(w.Id, encryptionKey))
            .Tap(w => uiServices.ActiveWallet.Current = w.AsMaybe());
    }

    public string CreateWalletText => IsRecovery ? "Recover Wallet" : "Create Wallet";
    public string CreatingWalletText => IsRecovery ? "Recovering Wallet..." : "Creating Wallet...";

    public string TitleText =>
        IsRecovery ? "You are all set to recover your wallet" : "You are all set to create your wallet";

    public required bool IsRecovery { get; init; }

    public IObservable<bool> IsValid =>
        this.WhenAnyValue<SummaryAndCreationViewModel, IWallet>(x => x.Wallet).NotNull();

    public IObservable<bool> IsBusy => CreateWallet.IsExecuting;
    public bool AutoAdvance => true;
    public ReactiveCommand<Unit, Result<IWallet>> CreateWallet { get; }
    public Maybe<string> Passphrase { get; }
    public Maybe<string> Title => "Summary";
}