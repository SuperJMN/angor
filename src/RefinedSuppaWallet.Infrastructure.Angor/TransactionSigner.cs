using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces.Transaction;
using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class TransactionSigner : ITransactionSigner
{
    public Task<Result<SignedTransaction>> Sign(WalletId walletId, UnsignedTransaction unsignedTransaction)
    {
        throw new NotImplementedException();
    }
}