using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class TransactionPreview(WalletId walletId, long amount, string address, long feeRate, Fee fee, WalletAppService walletAppService, IPassphraseProvider passphraseProvider) : IUnsignedTransaction
{
    public WalletId WalletId { get; } = walletId;
    public long Amount { get; } = amount;
    public string Address { get; } = address;
    public WalletAppService WalletAppService { get; } = walletAppService;
    public long TotalFee { get; set; } = fee.Value;

    public Task<Result<TxId>> Broadcast()
    {
        return passphraseProvider.Provide(WalletId)
            .ToResult("Passphrase must be provided")
            .Bind(passphrase => WalletAppService
                .SendAmount(WalletId, new Amount(Amount), new Address(Address), new FeeRate(feeRate), passphrase)
                .Map(id => id));
    }
}