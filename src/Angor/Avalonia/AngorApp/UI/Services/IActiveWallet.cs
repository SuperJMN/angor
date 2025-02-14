using Angor.UI.Model;
using Angor.UI.Model.Wallet;
using CSharpFunctionalExtensions;

namespace AngorApp.UI.Services;

public interface IActiveWallet
{
    Maybe<IWallet> Current { get; set; }
    IObservable<IWallet> CurrentChanged { get; }
    IObservable<bool> HasWallet { get; }
}