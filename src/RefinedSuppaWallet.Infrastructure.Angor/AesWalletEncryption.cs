using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;

namespace RefinedSuppaWallet.Infrastructure.Angor;

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