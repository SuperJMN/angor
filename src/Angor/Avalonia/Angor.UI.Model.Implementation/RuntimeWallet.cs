using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CSharpFunctionalExtensions;
using DynamicData;
using DynamicData.Aggregation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using Zafiro.CSharpFunctionalExtensions;

namespace Angor.UI.Model.Implementation;

public partial class RuntimeWallet : ReactiveObject, IWallet
{
    private readonly WalletAppService walletAppService;
    private readonly IWalletUnlocker walletUnlocker;

    public RuntimeWallet(WalletId walletId, WalletAppService walletAppService, IWalletUnlocker walletUnlocker)
    {
        Id = walletId;
        this.walletAppService = walletAppService;
        this.walletUnlocker = walletUnlocker;

        var transactionsChangeSet = Observable.FromAsync(() => walletAppService.GetTransactions(walletId))
            .Successes()
            .ToObservableChangeSet()
            .Replay()
            .RefCount();

        transactionsChangeSet
            .Transform(transaction => (IBroadcastedTransaction)new BroadcastedTransactionImpl(transaction))
            .Bind(out var transactions)
            .Subscribe();

        History = transactions;

        balanceHelper = transactionsChangeSet.Sum(x => x.Balance.Value).ToProperty(this, x => x.Balance);
        IsUnlocked = this.walletUnlocker.WalletUnlocked.Select(id => Id == id).StartWith(walletUnlocker.IsUnlocked(Id));
    }

    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; }

    [ObservableAsProperty] private long balance;

    public BitcoinNetwork Network { get; } = BitcoinNetwork.Testnet;
    public string ReceiveAddress { get; } = "";

    public Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate)
    {
        return walletAppService.EstimateFee(Id, new Amount(amount), new Address(address), new FeeRate(feerate))
            .Map(IUnsignedTransaction (fee) => new TransactionPreview(Id, amount, address, feerate, fee, walletAppService, walletUnlocker));
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success();
    }

    public IObservable<bool> IsUnlocked { get; }
    public WalletId Id { get; }
}