using AngorApp.Core;
using CSharpFunctionalExtensions;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class AngorWalleteRepository : IWalletRepository
{
    private readonly IStore store;
    private readonly IPassphraseProvider passphraseProvider;
    private readonly Dictionary<WalletId, Wallet> wallets = new();
    private readonly Dictionary<WalletId, string> names = new();
    private readonly Dictionary<WalletId, string> passphrases = new();

    public AngorWalleteRepository(IStore store, IPassphraseProvider passphraseProvider)
    {
        this.store = store;
        this.passphraseProvider = passphraseProvider;
        var walletId = WalletId.New();
        wallets.Add(walletId, new Wallet(walletId, SampleData.WalletDescriptor()));
    }
    
    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return wallets.Select(pair => (pair.Key, names.TryFind(pair.Key).GetValueOrDefault("")));
    }

    public async Task<Maybe<Wallet>> Get(WalletId id)
    {
        return await passphraseProvider.Provide(id).Bind(passphrase => wallets.TryFind(id));
    }

    public async Task<Result<Wallet>> ImportWallet(string name, string seed, string passphrase, Network network)
    {
        var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, network);
        var walletId = WalletId.New();
        var wallet = new Wallet(walletId, descriptor);
        
        wallets.Add(walletId, wallet);
        names.Add(walletId, name);

        return await Save().Map(() => wallet);
    }

    private async Task<Result> Save()
    {
        return Result.Success();
    }
}