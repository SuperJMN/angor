using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;

namespace AngorApp.Core;

internal class MasterkeyProvider : IMasterKeyProvider
{
    private readonly ISensitiveWalletDataProvider sensitiveWalletDataProvider;

    public MasterkeyProvider(ISensitiveWalletDataProvider sensitiveWalletDataProvider)
    {
        this.sensitiveWalletDataProvider = sensitiveWalletDataProvider;
    }
    
    public Task<Result<ExtKey>> GetMasterKey(WalletId walletId)
    {
        return sensitiveWalletDataProvider.RequestSensitiveData(walletId)
                .Map(tuple => new Mnemonic(tuple.seed).DeriveExtKey(tuple.passphrase));
    }
}