using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Angor.UI.Model;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Wallet.Operate;
using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;
using BitcoinNetwork = Angor.UI.Model.BitcoinNetwork;

namespace AngorApp.Sections.Wallet;

public class WalletDesign : IWallet
{
    public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; } = new ReadOnlyObservableCollection<IBroadcastedTransaction>([
        new BroadcastedTransactionDesign { Address = "someaddress1", Amount = 1000, UtxoCount = 12, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress2", Amount = 3000, UtxoCount = 15, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress3", Amount = 43000, UtxoCount = 15, Path = "path", ViewRawJson = "json" },
        new BroadcastedTransactionDesign { Address = "someaddress4", Amount = 30000, UtxoCount = 15, Path = "path", ViewRawJson = "json" }
    ]);

    public long Balance { get; } = 5_0000_0000;

    public string ReceiveAddress { get; } = SampleData.TestNetBitcoinAddress;

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
        var value = BitcoinAddressValidator.ValidateBitcoinAddress(address, Network);
        return value.IsValid ? Result.Success() : Result.Failure(value.Message);
    }

    public bool IsUnlocked { get; set; }
    public WalletId Id { get; }

    public BitcoinNetwork Network => BitcoinNetwork.Testnet;
}