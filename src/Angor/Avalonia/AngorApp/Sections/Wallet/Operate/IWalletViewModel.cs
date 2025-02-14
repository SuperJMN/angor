using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Wallet;

namespace AngorApp.Sections.Wallet.Operate;

public interface IWalletViewModel
{
    public IWallet Wallet { get; }
    public ICommand Send { get; }
}