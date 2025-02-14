using Angor.UI.Model.Wallet;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public interface IWalletSectionViewModel
{
    ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
}