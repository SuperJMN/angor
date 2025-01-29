using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application.Services.Wallet;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class TransactionPreview(WalletId walletId, long amount, string address, long feeRate, Fee fee, WalletAppService walletAppService) : IUnsignedTransaction
{
    public WalletId WalletId { get; } = walletId;
    public long Amount { get; } = amount;
    public string Address { get; } = address;
    public WalletAppService WalletAppService { get; } = walletAppService;
    public long TotalFee { get; set; } = fee.Value;
    public Task<Result<TxId>> Broadcast()
    {
        return WalletAppService
            .SendAmount(WalletId, new Amount(Amount), new Address(Address), new FeeRate(feeRate))
            .Map(id => id);
    }
}