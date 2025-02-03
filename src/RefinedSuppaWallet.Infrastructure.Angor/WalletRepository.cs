using CSharpFunctionalExtensions;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class AngorRepository : IWalletRepository
{
    private readonly IStore store;
    private readonly Dictionary<WalletId, Wallet> wallets = new();

    public AngorRepository(IStore store)
    {
        this.store = store;
    }
    
    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return [];
    }

    public async Task<Maybe<Wallet>> Get(WalletId id)
    {
        return Maybe.None;
    }

    public async Task<Result<Wallet>> ImportWallet(string seed, string passphrase, Network network)
    {
        var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, network);
        var walletId = WalletId.New();
        var wallet = new Wallet(walletId, descriptor);
        wallets.Add(walletId, wallet);

        return await Save().Map(() => wallet);
    }

    private async Task<Result> Save()
    {
        return Result.Success();
    }

    public Task<Result> Add(WalletId walletId, WalletDescriptor descriptor)
    {
        throw new NotImplementedException();
    }
}