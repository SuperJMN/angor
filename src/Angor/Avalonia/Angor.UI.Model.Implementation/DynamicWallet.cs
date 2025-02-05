using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using CSharpFunctionalExtensions;
using DynamicData;
using DynamicData.Aggregation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;
using Zafiro.UI;

namespace Angor.UI.Model.Implementation;

public partial class DynamicWallet : ReactiveObject, IWallet
{
    private readonly WalletAppService walletAppService;

    public DynamicWallet(WalletId walletId, WalletAppService walletAppService, IWalletUnlocker walletUnlocker)
    {
        Id = walletId;
        this.walletAppService = walletAppService;

        var transactionsSource = new SourceCache<BroadcastedTransaction, string>(x => x.Id);

        var changes = transactionsSource.Connect();
        
        changes
            .Transform(transaction => (IBroadcastedTransaction)new BroadcastedTransactionImpl(transaction))
            .Bind(out var transactions)
            .Subscribe();
        
        History = transactions;

        balanceHelper = changes.Sum(x => x.Balance.Value).ToProperty(this, x => x.Balance);
        
        LoadTransactions = ReactiveCommand.CreateFromTask(async () =>
        {
            return await walletAppService.GetTransactions(Id).Tap(t => transactionsSource.AddOrUpdate(t)).Bind(_ => Result.Success());
        });

        Load = ReactiveCommand.CreateCombined([LoadTransactions]);
        
        isUnlockedHelper = walletUnlocker.WalletUnlocked.Select(id => Id == id).StartWith(walletUnlocker.IsUnlocked(Id)).ToProperty(this, x => x.IsUnlocked);
    }

    
    [ObservableAsProperty] private bool isUnlocked; 

    public CombinedReactiveCommand<Unit, Result> Load { get;  }

    public ReactiveCommand<Unit, Result> LoadTransactions { get; }

    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; }

    [ObservableAsProperty] private long balance;

    public BitcoinNetwork Network { get; } = BitcoinNetwork.Testnet;
    public string ReceiveAddress { get; } = "";

    public Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate)
    {
        return walletAppService.EstimateFee(Id, new Amount(amount), new Address(address), new FeeRate(feerate))
            .Map(IUnsignedTransaction (fee) => new TransactionPreview(Id, amount, address, feerate, fee, walletAppService));
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success()
            .Ensure(() =>
            {
                var validateBitcoinAddress = BitcoinAddressValidator.ValidateBitcoinAddress(address, Network);
                return validateBitcoinAddress.Network == Network;
            }, "Network mismatch");
    }

    //public IObservable<bool> IsUnlocked { get; }
    public WalletId Id { get; }
}