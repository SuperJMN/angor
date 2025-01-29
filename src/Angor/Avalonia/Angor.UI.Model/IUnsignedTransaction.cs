using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model;

public interface IUnsignedTransaction
{
    public long TotalFee { get; set; }
    Task<Result<TxId>> Broadcast();
}