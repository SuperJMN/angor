namespace SuppaWallet.Domain;

public record Balance(long Value)
{
    public bool IsSufficient(Amount amount) => Value >= amount.Value;   
}