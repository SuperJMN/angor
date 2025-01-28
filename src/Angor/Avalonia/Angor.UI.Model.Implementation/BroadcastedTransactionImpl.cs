using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class BroadcastedTransactionImpl : IBroadcastedTransaction
{
    public BroadcastedTransactionImpl(BroadcastedTransaction transaction)
    {
        Amount = transaction.Balance.Value;
        Id = transaction.Id;
    }

    public string Id { get; }

    public string Address { get; }
    public decimal FeeRate { get; }
    public decimal TotalFee { get;  }
    public long Amount { get; }
    public string Path { get; }
    public int UtxoCount { get; }
    public string ViewRawJson { get; }
}