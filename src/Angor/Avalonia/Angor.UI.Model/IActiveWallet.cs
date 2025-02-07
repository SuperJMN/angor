using CSharpFunctionalExtensions;

namespace Angor.UI.Model;

public interface IActiveWallet
{
    Maybe<IWallet> Current { get; set; }
    IObservable<IWallet> CurrentChanged { get; }
}