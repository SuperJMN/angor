using System.Collections.ObjectModel;
using System.Reactive;
using CSharpFunctionalExtensions;
using ReactiveUI;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model;

public interface IWallet
{
    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; }
    long Balance { get; }
    public BitcoinNetwork Network { get; }
    public string ReceiveAddress { get; }
    Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate);
    Result IsAddressValid(string address);
    public bool IsUnlocked { get; }
    WalletId Id { get; }
    public CombinedReactiveCommand<Unit, Result> Load { get; }
}