using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services.Wallet;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class RuntimeWallet : IWallet
{
    private readonly WalletId walletId;
    private readonly WalletAppService walletAppService;

    public RuntimeWallet(WalletId walletId, WalletAppService walletAppService)
    {
        this.walletId = walletId;
        this.walletAppService = walletAppService;
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
        return walletAppService.EstimateFee(walletId, new Amount(amount), new Address(address), new FeeRate(feerate))
            .Map(IUnsignedTransaction (fee) => new TransactionPreview(walletId, amount, address, feerate, fee, walletAppService));
    }

    public Result IsAddressValid(string address)
    {
        return Result.Success();
    }
}