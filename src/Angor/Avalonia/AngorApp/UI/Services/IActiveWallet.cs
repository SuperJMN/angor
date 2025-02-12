using Angor.UI.Model;
using CSharpFunctionalExtensions;
using SuppaWallet.Gui.Model;

namespace AngorApp.UI.Services;

public interface IActiveWallet
{
    Maybe<IWallet> Current { get; set; }
    IObservable<IWallet> CurrentChanged { get; }
    IObservable<bool> HasWallet { get; }
}