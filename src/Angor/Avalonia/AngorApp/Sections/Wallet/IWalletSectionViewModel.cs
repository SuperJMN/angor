using CSharpFunctionalExtensions;
using SuppaWallet.Gui.Model;

namespace AngorApp.Sections.Wallet;

public interface IWalletSectionViewModel
{
    ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
}