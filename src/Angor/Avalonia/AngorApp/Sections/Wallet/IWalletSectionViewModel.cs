using System.Windows.Input;
using Angor.UI.Model;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public interface IWalletSectionViewModel
{
    ReactiveCommand<Unit, Maybe<IWallet>> CreateWallet { get; }
    ReactiveCommand<Unit, Maybe<IWallet>> RecoverWallet { get; }
    IObservable<bool> IsBusy { get; set; }
}