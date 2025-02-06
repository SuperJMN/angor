using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public interface ISensitiveWalletDataProvider
{
    Task<Result<(string seed, string passphrase)>> RequestSensitiveData(WalletId id);
}