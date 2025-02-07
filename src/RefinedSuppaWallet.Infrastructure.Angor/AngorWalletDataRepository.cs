using System.Text.Json;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class AngorWalletDataRepository : IWalletRepository, IWalletImporter, ISensitiveWalletDataProvider
{
    private readonly IStore store;
    private readonly IWalletUnlockHandler walletUnlockHandler;
    private readonly IWalletEncryption walletEncryption;
    private readonly IPassphraseProvider passphraseProvider;
    private readonly IEncryptionKeyProvider encryptionKeyProvider;

    private readonly Dictionary<WalletId, Wallet> wallets = new();

    // Se deja en null hasta el primer acceso, para cargar de forma perezosa
    private Dictionary<Guid, EncryptedWallet>? encryptedWallets;

    public AngorWalletDataRepository(IStore store, IWalletUnlockHandler walletUnlockHandler, IWalletEncryption walletEncryption, IPassphraseProvider passphraseProvider, IEncryptionKeyProvider encryptionKeyProvider)
    {
        this.store = store;
        this.walletUnlockHandler = walletUnlockHandler;
        this.walletEncryption = walletEncryption;
        this.passphraseProvider = passphraseProvider;
        this.encryptionKeyProvider = encryptionKeyProvider;
    }

    /// <summary>
    /// Se asegura de que el diccionario de wallets cifradas esté cargado.
    /// </summary>
    private async Task EnsureEncryptedWalletsLoaded()
    {
        if (encryptedWallets is null)
        {
            encryptedWallets = await store.Load<List<EncryptedWallet>>("wallets.json")
                .Map(list => list.ToDictionary(x => x.Id))
                .GetValueOrDefault(() => new Dictionary<Guid, EncryptedWallet>());
        }
    }

    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        await EnsureEncryptedWalletsLoaded();
        return encryptedWallets!.Select(pair => (new WalletId(pair.Key), pair.Value.Name));
    }

    public async Task<Result<Wallet>> Get(WalletId id)
    {
        await EnsureEncryptedWalletsLoaded();

        if (wallets.TryGetValue(id, out var cachedWallet))
            return Result.Success(cachedWallet);

        if (!encryptedWallets!.TryGetValue(id.Id, out var encryptedWallet))
            return Result.Failure<Wallet>("Wallet not found");

        var passwordMaybe = await walletUnlockHandler.RequestPassword(id);
        var passwordResult = passwordMaybe.ToResult("No password provided");

        return await passwordResult
            .Bind(password => Decrypt(id, encryptedWallet, password))
            .Map(descriptor =>
            {
                var newWallet = new Wallet(id, descriptor);
                wallets[id] = newWallet;
                return newWallet;
            });
    }

    private Task<Result<WalletDescriptor>> Decrypt(WalletId id, EncryptedWallet wallet, string password)
    {
        return walletEncryption.Decrypt(wallet, password)
            .MapError(_ => "Invalid decryption password")
            .MapTry(walletData => JsonSerializer.Deserialize<WalletDescriptorDto>(walletData.DescriptorJson))
            .EnsureNotNull("Invalid wallet data")
            .Bind(dto => dto.ToDomain())
            .Tap(() => walletUnlockHandler.ConfirmUnlock(id, password));
    }

    public async Task<Result<Wallet>> ImportWallet(
        string name,
        string seedwords,
        Maybe<string> passphrase,
        string encryptionKey,
        BitcoinNetwork network)
    {
        await EnsureEncryptedWalletsLoaded();

        var walletId = WalletId.New();
        // Crea el descriptor usando seedwords y passphrase (si la hay)
        var descriptor = WalletDescriptorFactory.Create(seedwords, passphrase, network.ToNBitcoin());
        var wallet = new Wallet(walletId, descriptor);

        // Serializa el descriptor a JSON a través de un DTO
        var dto = descriptor.ToDto();
        var descriptorJson = JsonSerializer.Serialize(dto);

        // Crea el WalletData incluyendo también las seedwords
        var walletData = new WalletData
        {
            DescriptorJson = descriptorJson,
            RequiresPassphrase = passphrase.HasValue,
            SeedWords = seedwords
        };

        // Cifra la información y la guarda
        var encryptedWallet = await walletEncryption.Encrypt(walletData, encryptionKey, name, walletId.Id);

        wallets[walletId] = wallet;
        encryptedWallets![walletId.Id] = encryptedWallet;

        return await Save().Map(() => wallet);
    }

    public async Task<Result<(string seed, string passphrase)>> RequestSensitiveData(WalletId id)
    {
        if (!encryptedWallets!.TryGetValue(id.Id, out var encryptedWallet))
            return Result.Failure<(string, string)>("Wallet not found");

        return await encryptionKeyProvider.GetEncryptionKey(id)
            .Bind(encryptionKey => passphraseProvider.RequestPassphrase(id)
                .Bind(passphrase => walletEncryption.Decrypt(encryptedWallet, encryptionKey)
                    .Ensure(data => data.SeedWords != null, "Seed words not found")
                    .Ensure(data => data.SeedWords != null, "Descriptor not found")
                    .Map(data => (data.SeedWords!, passphrase))));
    }

    private async Task<Result> Save()
    {
        try
        {
            // encryptedWallets ya ha sido cargado previamente
            var walletsList = encryptedWallets!.Values.ToList();
            await store.Save("wallets.json", walletsList);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error saving wallets: {ex.Message}");
        }
    }
}