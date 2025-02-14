namespace SuppaWallet.Gui.Model;

public interface IBroadcastedTransaction
{
    public string Address { get; }
    public decimal FeeRate { get; }
    public decimal TotalFee { get; }
    public long Amount { get; }
    public string Path { get; }
    public int UtxoCount { get; }
    public string ViewRawJson { get; }
    string Id { get; }
}