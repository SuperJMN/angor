namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.Summary;

public class SummaryViewModelDesign : ISummaryViewModel
{
    public Maybe<string> Passphrase { get; } = Maybe<string>.None;
    public ReactiveCommand<Unit, Result<IWallet>> CreateWallet { get; }
    public string CreateWalletText => IsRecovery ? "Recover Wallet" : "Create Wallet";
    public string CreatingWalletText => IsRecovery ? "Recovering Wallet..." : "Creating Wallet...";
    public bool IsRecovery { get; set; }
    public string TitleText => IsRecovery ? "You are all set to recover your wallet" : "You are all set to create your wallet";
}