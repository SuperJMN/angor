using CSharpFunctionalExtensions;

namespace SuppaWallet.Domain;

public static class WalletDomainService
{
    public static Result<Balance> CalculateBalance(IEnumerable<BroadcastedTransaction> transactions)
    {
        var sum = transactions.Sum(t => t.Balance.Value);
        return Result.Success(new Balance(sum));
    }
}