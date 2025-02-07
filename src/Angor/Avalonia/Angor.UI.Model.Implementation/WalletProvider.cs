using System.Reactive.Linq;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;

namespace Angor.UI.Model.Implementation;

public class ActiveWallet : IActiveWallet
{
    private readonly BehaviorSubject<Maybe<IWallet>> currentWallet = new(Maybe<IWallet>.None);
    public IObservable<bool> HasWallet => currentWallet.Any();

    public Maybe<IWallet> Current
    {
        get => currentWallet.Value;
        set => currentWallet.OnNext(value);
    }

    public IObservable<IWallet> CurrentChanged => currentWallet.Values().AsObservable();
}