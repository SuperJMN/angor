using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace AngorApp.Core;

internal class TransactionSigner : ITransactionSigner
{
    public Task<Result<SignedTransaction>> Sign(UnsignedTransaction signedTransaction, string passphrase)
    {
        throw new NotImplementedException();
    }
}