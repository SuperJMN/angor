namespace SuppaWallet.Domain;

public class UnsignedTransaction(string hex, Fee fee, IReadOnlyList<PrevCoinData> prevCoins)
{
    public string Hex { get; } = hex;
    public Fee Fee { get; } = fee;
    public IReadOnlyList<PrevCoinData> PrevCoins { get; } = prevCoins;
}