using System.Reactive.Linq;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;
using Zafiro.CSharpFunctionalExtensions;

namespace Angor.UI.Model.Implementation;

public class WalletProvider : IWalletProvider
{
    private readonly BehaviorSubject<Maybe<IWallet>> currentWallet = new(Maybe<IWallet>.None);
    public IObservable<bool> HasWallet => currentWallet.Any();

    public Maybe<IWallet> CurrentWallet
    {
        get => currentWallet.Value;
        set => currentWallet.OnNext(value);
    }

    public IObservable<IWallet> CurrentWallets => currentWallet.Values().AsObservable();
}