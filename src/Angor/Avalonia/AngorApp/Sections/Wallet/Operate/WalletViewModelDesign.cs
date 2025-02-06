using System.Windows.Input;
using Angor.UI.Model;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.Operate;

public class WalletViewModelDesign : IWalletViewModel
{
    public IWallet Wallet { get; } = new WalletDesign();
    public ICommand Send { get; }
    public ReactiveCommand<Unit, Result<RefinedSuppaWallet.Domain.Wallet>> Unlock { get; }
}