using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services.Wallet;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class RuntimeWallet : IWallet
{
    public RuntimeWallet(WalletId walletId, WalletAppService walletAppService)
    {
        History = walletAppService
            .GetTransactions(walletId).Map(collection => collection.Select(IBroadcastedTransaction (transaction) => new BroadcastedTransactionImpl(transaction))).GetValueOrDefault();
        Balance = walletAppService.GetBalance(walletId).Map(x => x.Value).GetValueOrDefault();
    }

    public IEnumerable<IBroadcastedTransaction> History { get; }
    
    public long? Balance { get; }

    public BitcoinNetwork Network { get; } = BitcoinNetwork.Testnet;
    public string ReceiveAddress { get; } = "";
    
    public Task<Result<IUnsignedTransaction>> CreateTransaction(long amount, string address, long feerate)
    {
        throw new NotImplementedException();
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success();
    }
}