using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;
using RefinedSuppaWallet.Infrastructure.Angor.Store;

public interface IWalletEncryption
{
    Task<Result<string>> Decrypt(EncryptedWallet wallet, string encryptionKey);
    Task<EncryptedWallet> Encrypt(string seed, string encryptionKey, string name, Guid id);
}

public class AesWalletEncryption : IWalletEncryption
{
    private const int ITERATIONS = 100000;
    private const int KEY_SIZE = 256;

    public async Task<Result<string>> Decrypt(EncryptedWallet encryptedWallet, string encryptionKey)
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
            return Result.Success(await reader.ReadToEndAsync());
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Error decrypting wallet: {ex.Message}");
        }
    }

    public async Task<EncryptedWallet> Encrypt(string seed, string encryptionKey, string name, Guid id)
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
            using (var writer = new BinaryWriter(csEncrypt))
            {
                writer.Write(Encoding.UTF8.GetBytes(seed));
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
            .Bind(password => walletEncryption.Decrypt(encryptedWallet, password).Tap(() => walletUnlocker.ConfirmUnlock(id, password)))
            .Map(seed =>
            {
                var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, Network.Main);
                var newWallet = new Wallet(id, descriptor);
                wallets[id] = newWallet;
                return newWallet;
            });
    }

    public async Task<Result<Wallet>> ImportWallet(string name, string seed, string encryptionKey, BitcoinNetwork network)
    {
        await EnsureEncryptedWalletsLoaded();

        var walletId = WalletId.New();
        var descriptor = WalletDescriptorFactory.CreateFromSeed(seed, network.ToNBitcoin());
        var wallet = new Wallet(walletId, descriptor);

        var encryptedWallet = await walletEncryption.Encrypt(seed, encryptionKey, name, walletId.Id);

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