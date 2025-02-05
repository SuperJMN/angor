using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class WalletData
{
    public string Seed { get; set; }
    public bool RequiresPassphrase { get; set; }
}

public interface IWalletEncryption
{
    Task<Result<WalletData>> Decrypt(EncryptedWallet wallet, string encryptionKey);
    Task<EncryptedWallet> Encrypt(WalletData walletData, string encryptionKey, string name, Guid id);
}

public class AesWalletEncryption : IWalletEncryption
{
    private const int ITERATIONS = 100000;
    private const int KEY_SIZE = 256;

    public async Task<Result<WalletData>> Decrypt(EncryptedWallet encryptedWallet, string encryptionKey)
    {
        try
        {
            var salt = Convert.FromBase64String(encryptedWallet.Salt);
            var encryptedData = Convert.FromBase64String(encryptedWallet.EncryptedData);
            var iv = Convert.FromBase64String(encryptedWallet.IV);

            using var deriveBytes = new Rfc2898DeriveBytes(
                encryptionKey,
                salt,
                ITERATIONS,
                HashAlgorithmName.SHA256);
            var key = deriveBytes.GetBytes(KEY_SIZE / 8);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var msDecrypt = new MemoryStream(encryptedData);
            using var csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(csDecrypt);
            var jsonData = await reader.ReadToEndAsync();
            return Result.Success(System.Text.Json.JsonSerializer.Deserialize<WalletData>(jsonData)!);
        }
        catch (Exception ex)
        {
            return Result.Failure<WalletData>($"Error decrypting wallet: {ex.Message}");
        }
    }

    public async Task<EncryptedWallet> Encrypt(WalletData walletData, string encryptionKey, string name, Guid id)
    {
        var salt = GenerateRandomBytes(32);
        var iv = GenerateRandomBytes(16);

        using var deriveBytes = new Rfc2898DeriveBytes(
            encryptionKey,
            salt,
            ITERATIONS,
            HashAlgorithmName.SHA256);
        var key = deriveBytes.GetBytes(KEY_SIZE / 8);

        byte[] encryptedData;
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(csEncrypt, Encoding.UTF8))
            {
                var jsonData = System.Text.Json.JsonSerializer.Serialize(walletData);
                await writer.WriteAsync(jsonData);
            }
            encryptedData = msEncrypt.ToArray();
        }

        return new EncryptedWallet
        {
            Id = id,
            Name = name,
            Salt = Convert.ToBase64String(salt),
            IV = Convert.ToBase64String(iv),
            EncryptedData = Convert.ToBase64String(encryptedData)
        };
    }

    private static byte[] GenerateRandomBytes(int length)
    {
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return randomBytes;
    }
}

public class AngorWalletRepository : IWalletRepository, IWalletImporter
{
    private readonly IStore store;
    private readonly IWalletUnlocker walletUnlocker;
    private readonly IWalletEncryption walletEncryption;
    private readonly Dictionary<WalletId, Wallet> wallets = new();
    // Se deja en null hasta el primer acceso, para cargar de forma perezosa
    private Dictionary<Guid, EncryptedWallet>? encryptedWallets;

    public AngorWalletRepository(IStore store, IWalletUnlocker walletUnlocker, IWalletEncryption walletEncryption)
    {
        this.store = store;
        this.walletUnlocker = walletUnlocker;
        this.walletEncryption = walletEncryption;
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

        var passwordMaybe = await walletUnlocker.Provide(id);
        var passwordResult = passwordMaybe.ToResult("No password provided");

        return await passwordResult
            .Bind(password => Decrypt(id, encryptedWallet, password))
            .Map(seed =>
            {
                var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, Network.TestNet);
                var newWallet = new Wallet(id, descriptor);
                wallets[id] = newWallet;
                return newWallet;
            });
    }

    private Task<Result<string>> Decrypt(WalletId id, EncryptedWallet wallet, string password)
    {
        return walletEncryption.Decrypt(wallet, password)
            .Map(walletData => walletData.Seed)
            .MapError(_ => "Invalid decryption password")
            .Tap(() => walletUnlocker.ConfirmUnlock(id, password));
    }

    public async Task<Result<Wallet>> ImportWallet(string name, string seed, string encryptionKey, BitcoinNetwork network, bool requiresPassphrase = false)
    {
        await EnsureEncryptedWalletsLoaded();

        var walletId = WalletId.New();
        var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, network.ToNBitcoin());
        var wallet = new Wallet(walletId, descriptor);

        var walletData = new WalletData
        {
            Seed = seed,
            RequiresPassphrase = requiresPassphrase
        };

        var encryptedWallet = await walletEncryption.Encrypt(walletData, encryptionKey, name, walletId.Id);

        wallets[walletId] = wallet;
        encryptedWallets![walletId.Id] = encryptedWallet;

        return await Save().Map(() => wallet);
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

public class EncryptedWallet
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EncryptedData { get; set; }
    public string Salt { get; set; }
    public string IV { get; set; }
}