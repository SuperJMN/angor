using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Domain;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Wallet.Operate;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet;

public class WalletDesign : IWallet
{
    private static readonly IEnumerable<IBroadcastedTransaction> history =
    [
        new BroadcastedTransactionDesign { Address = "someaddress1", Amount = 1000, UtxoCount = 12, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress2", Amount = 3000, UtxoCount = 15, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress3", Amount = 43000, UtxoCount = 15, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress4", Amount = 30000, UtxoCount = 15, Path = "path", ViewRawJson = "json" }
    ];
    
    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; } = new(new ObservableCollection<IBroadcastedTransaction>(history));

    public long Balance { get; set; } = 5_0000_0000;

    public CombinedReactiveCommand<Unit, Result> Load { get; }
    public string ReceiveAddress { get; } = SampleData.TestNetBitcoinAddress;
    public ReactiveCommand<Unit, Result<string>> GenerateReceiveAddress { get; }

    public async Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate)
    {
        await Task.Delay(1000);

        //return Result.Failure<ITransaction>("Transaction creation failed");
        return new UnsignedTransactionDesign
        {
            Address = address,
            Amount = amount,
            TotalFee = feerate * 100,
            FeeRate = feerate
        };
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success();
    }

    public BitcoinNetwork Network => BitcoinNetwork.Testnet;
    public bool IsUnlocked { get; } = true;
    public WalletId Id { get; } = WalletId.New();
}