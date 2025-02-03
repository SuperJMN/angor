using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CSharpFunctionalExtensions;
using DynamicData;
using DynamicData.Aggregation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using Zafiro.CSharpFunctionalExtensions;

namespace Angor.UI.Model.Implementation;

public partial class RuntimeWallet : ReactiveObject, IWallet
{
    private readonly WalletId walletId;
    private readonly WalletAppService walletAppService;
    private readonly IPassphraseProvider passphraseProvider;

    public RuntimeWallet(WalletId walletId, WalletAppService walletAppService, IPassphraseProvider passphraseProvider)
    {
        Id = walletId;
        this.walletAppService = walletAppService;
        this.passphraseProvider = passphraseProvider;

        var transactionsChangeSet = Observable.FromAsync(() => walletAppService.GetTransactions(walletId))
            .Successes()
            .ToObservableChangeSet();

        transactionsChangeSet
            .Transform(transaction => (IBroadcastedTransaction)new BroadcastedTransactionImpl(transaction))
            .Bind(out var transactions)
            .Subscribe();

        History = transactions;

        balanceHelper = transactionsChangeSet.Sum(x => x.Balance.Value).ToProperty(this, x => x.Balance);
    }

    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; }

    [ObservableAsProperty] private long balance;

    public BitcoinNetwork Network { get; } = BitcoinNetwork.Testnet;
    public string ReceiveAddress { get; } = "";

    public Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate)
    {
        return walletAppService.EstimateFee(walletId, new Amount(amount), new Address(address), new FeeRate(feerate))
            .Map(IUnsignedTransaction (fee) => new TransactionPreview(walletId, amount, address, feerate, fee, walletAppService, passphraseProvider));
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success();
    }

    public bool IsUnlocked { get; set; }
    public WalletId Id { get; }
}