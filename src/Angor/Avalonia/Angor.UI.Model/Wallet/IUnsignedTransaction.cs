using CSharpFunctionalExtensions;
using SuppaWallet.Domain;

namespace SuppaWallet.Gui.Model;

public interface IUnsignedTransaction
{
    public long TotalFee { get; set; }
    Task<Result<TxId>> Accept();
}