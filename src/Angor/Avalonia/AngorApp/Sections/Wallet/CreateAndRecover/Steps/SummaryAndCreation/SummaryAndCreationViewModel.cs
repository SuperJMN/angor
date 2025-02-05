using System.Linq;
using System.Threading.Tasks;
using Angor.UI.Model;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Helpers;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;
using BitcoinNetwork = RefinedSuppaWallet.Domain.BitcoinNetwork;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public partial class SummaryAndCreationViewModel : ReactiveValidationObject, IStep, ISummaryAndCreationViewModel
{
    private readonly IWalletImporter walletImporter;
    private readonly IWalletUnlocker unlocker;
    private readonly IWalletProvider walletProvider;
    private readonly IWalletBuilder walletBuilder;
    [ObservableAsProperty] private IWallet? wallet;

    public SummaryAndCreationViewModel(IWalletImporter walletImporter, IWalletUnlocker unlocker, IWalletProvider walletProvider, Maybe<string> passphrase, SeedWords seedwords, string encryptionKey, IWalletBuilder walletBuilder,
        Action<Result<IWallet>> creationResult)
    {
        this.walletImporter = walletImporter;
        this.unlocker = unlocker;
        this.walletProvider = walletProvider;
        this.walletBuilder = walletBuilder;
        Passphrase = passphrase;
        CreateWallet = ReactiveCommand.CreateFromTask(() => CreateAndSet(seedwords, passphrase, encryptionKey));
        walletHelper = CreateWallet.Successes().ToProperty(this, x => x.Wallet);
        CreateWallet.Subscribe(result => creationResult(result));
    }

    private Task<Result<IWallet>> CreateAndSet(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return walletImporter.ImportWallet("Main", string.Join(" ", seedwords), encryptionKey, BitcoinNetwork.Testnet, passphrase.Match(_ => true, () => false))
            .Bind(w => walletBuilder.Create(w.Id))
            .Tap(w => unlocker.ConfirmUnlock(w.Id, encryptionKey))
            .Tap(w => walletProvider.CurrentWallet = w.AsMaybe());
    }

    public string CreateWalletText => IsRecovery ? "Recover Wallet" : "Create Wallet";
    public string CreatingWalletText => IsRecovery ? "Recovering Wallet..." : "Creating Wallet...";
    public string TitleText => IsRecovery ? "You are all set to recover your wallet" : "You are all set to create your wallet";
    public required bool IsRecovery { get; init; }

    public IObservable<bool> IsValid => this.WhenAnyValue<SummaryAndCreationViewModel, IWallet>(x => x.Wallet).NotNull();
    public IObservable<bool> IsBusy => CreateWallet.IsExecuting;
    public bool AutoAdvance => true;
    public ReactiveCommand<Unit, Result<IWallet>> CreateWallet { get; }
    public Maybe<string> Passphrase { get; }
    public Maybe<string> Title => "Summary";
}