using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces.Wallet;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace AngorApp.Core;

internal class WalletRepository : IWalletRepository
{
    private readonly Func<IProtectedWalletRepository> inner;
    private readonly IPassphraseProvider provider;
    private readonly Dictionary<WalletId, Wallet> unlockedWallets = new();

    public WalletRepository(Func<IProtectedWalletRepository> inner, IPassphraseProvider provider)
    {
        this.inner = inner;
        this.provider = provider;
    }

    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return await inner().ListWallets();
    }

    public Task<Maybe<Wallet>> Get(WalletId id)
    {
        return unlockedWallets.TryFind(id).Or(() =>
        {
            var task = provider.Provide(id).Bind(passphrase =>
            {
                IProtectedWalletRepository protectedWalletRepository = inner();
                Task<Maybe<Wallet>> maybe = protectedWalletRepository.Get(id, passphrase);
                return maybe;
            });

            return task;
        });
    }

    public async Task<Result<Wallet>> ImportWallet(WalletId walletId, string walletName, WalletDescriptor descriptor, string passphrase)
    {
        return await inner().Add(walletId, walletName, descriptor, passphrase);
    }
}