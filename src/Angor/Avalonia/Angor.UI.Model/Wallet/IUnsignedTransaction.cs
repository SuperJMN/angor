using Angor.Wallet.Domain;
using CSharpFunctionalExtensions;

namespace Angor.UI.Model.Wallet;

public interface IUnsignedTransaction
{
    public long TotalFee { get; set; }
    Task<Result<TxId>> Accept();
}