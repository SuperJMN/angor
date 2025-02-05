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

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public partial class SummaryAndCreationViewModel : ReactiveValidationObject, IStep, ISummaryAndCreationViewModel
{
    private readonly IWalletImporter walletImporter;
    private readonly IWalletProvider walletProvider;
    private readonly IWalletBuilder walletBuilder;
    [ObservableAsProperty] private IWallet? wallet;

    public SummaryAndCreationViewModel(IWalletImporter walletImporter, IWalletProvider walletProvider, Maybe<string> passphrase, SeedWords seedwords, string encryptionKey, IWalletBuilder walletBuilder,
        Action<Result<IWallet>> creationResult)
    {
        this.walletImporter = walletImporter;
        this.walletProvider = walletProvider;
        this.walletBuilder = walletBuilder;
        Passphrase = passphrase;
        CreateWallet = ReactiveCommand.CreateFromTask(() => CreateAndSet(seedwords, passphrase, encryptionKey));
        walletHelper = CreateWallet.Successes().ToProperty(this, x => x.Wallet);
        CreateWallet.Subscribe(result => creationResult(result));
    }

    private Task<Result<IWallet>> CreateAndSet(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return walletBuilder.Create(WalletId.New())
            .Tap(w => walletImporter.ImportWallet("Main", string.Join(" ", seedwords), encryptionKey, w.Network.ToDomain()))
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