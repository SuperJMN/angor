using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model.Implementation;

public class WalletBuilder : IWalletBuilder
{
    private readonly WalletAppService walletAppService;
    private readonly IPassphraseProvider passphraseProvider;

    public WalletBuilder(WalletAppService walletAppService, IPassphraseProvider passphraseProvider)
    {
        this.walletAppService = walletAppService;
        this.passphraseProvider = passphraseProvider;
    }

    public async Task<Result<IWallet>> Create(SeedWords seedwords, Maybe<string> passphrase, string encryptionKey)
    {
        return new RuntimeWallet(WalletId.New(), walletAppService, passphraseProvider);
    }
}