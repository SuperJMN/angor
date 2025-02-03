using RefinedSuppaWalet.Infrastructure.Interfaces.Wallet;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;
using System.Text.Json;
using CSharpFunctionalExtensions;

namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class AngorWalletRepository : IProtectedWalletRepository
{
    private const string WalletsFile = "wallets.json";
    private readonly IStore store;
    private readonly AsyncLazy<ManyWalletsData> walletStore;

    public AngorWalletRepository(IStore store)
    {
        this.store = store;
        walletStore = new AsyncLazy<ManyWalletsData>(() => LoadWallets(this.store));
    }

    private static async Task<ManyWalletsData> LoadWallets(IStore store)
    {
        var data = await store.Load<ManyWalletsData>(WalletsFile);
        return data ?? new ManyWalletsData();
    }

    private async Task SaveWallets()
    {
        await store.Save(WalletsFile, walletStore);
    }

    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return (await walletStore.Value).Wallets.Select(x => (new WalletId(x.WalletId), x.WalletName));
    }

    public async Task<Maybe<Wallet>> Get(WalletId id, string passphrase)
    {
        var info = (await walletStore.Value).Wallets.FirstOrDefault(x => x.WalletId == id.Id);
        if (info == null)
            return Maybe<Wallet>.None;

        try
        {
            var json = WalletCrypto.Decrypt(info.EncryptedData, info.Salt, passphrase);
            var storedWallet = JsonSerializer.Deserialize<WalletData.StoredWallet>(json);
            if (storedWallet == null)
                return Maybe<Wallet>.None;

            return Maybe<Wallet>.From(MapToWallet(storedWallet));
        }
        catch
        {
            // passphrase incorrecta o datos corruptos
            return Maybe<Wallet>.None;
        }
    }

    public async Task<Result<Wallet>> Add(WalletId walletId, string walletName, WalletDescriptor descriptor, string passphrase)
    {
        var exists = (await walletStore.Value).Wallets.Any(w => w.WalletId == walletId.Id);
        if (exists)
            return Result.Failure<Wallet>("Wallet already exists.");

        var domainWallet = new Wallet(walletId, descriptor);

        var storedWallet = MapToStoredWallet(domainWallet);

        // serializamos la info sensible y la ciframos
        var json = JsonSerializer.Serialize(storedWallet);
        var (encrypted, salt) = WalletCrypto.Encrypt(json, passphrase);

        // creamos la entrada
        var newEntry = new EncryptedWalletInfo
        {
            WalletId = walletId.Id,
            WalletName = walletName, // en claro
            EncryptedData = encrypted, // cifrado
            Salt = salt
        };

        // añadimos a la lista y guardamos
        (await walletStore.Value).Wallets.Add(newEntry);
        await SaveWallets();

        return Result.Success(domainWallet);
    }

    private static Wallet MapToWallet(WalletData.StoredWallet stored)
    {
        var descriptor = WalletDescriptor.Create(
            stored.Fingerprint,
            stored.Network,
            XPubCollection.Create(
                XPub.Create(
                    stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Value,
                    ScriptType.SegWit,
                    DerivationPath.Create(
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.Purpose,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.CoinType,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.Account
                    )
                ),
                XPub.Create(
                    stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Value,
                    ScriptType.Taproot,
                    DerivationPath.Create(
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.Purpose,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.CoinType,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.Account
                    )
                )
            ));

        var wallet = new Wallet(new WalletId(stored.Id), descriptor);

        return wallet;
    }

    private static WalletData.StoredWallet MapToStoredWallet(Domain.Wallet wallet)
    {
        return new WalletData.StoredWallet
        {
            Id = wallet.Id.Id,
            Fingerprint = wallet.Descriptor.Fingerprint,
            Network = wallet.Descriptor.Network,
            XPubs = wallet.Descriptor.XPubs.Select(x => new WalletData.StoredXPub
            {
                Value = x.Value,
                ScriptType = x.ScriptType,
                Path = new WalletData.StoredDerivationPath
                {
                    Purpose = x.Path.Purpose,
                    CoinType = x.Path.CoinType,
                    Account = x.Path.Account
                }
            }).ToList()
        };
    }
}