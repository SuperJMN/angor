using System.Security.Cryptography;

namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class WalletCrypto
{
    private const int KeySize = 256;
    private const int BlockSize = 128;
    private const int Iterations = 100000;

    public static (string encryptedData, string salt) Encrypt(string data, string password)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;

        // Generar salt aleatorio
        var salt = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }

        // Derivar key del password
        var key = new Rfc2898DeriveBytes(password, salt, Iterations);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(data);
        }

        return (
            Convert.ToBase64String(msEncrypt.ToArray()),
            Convert.ToBase64String(salt)
        );
    }

    public static string Decrypt(string encryptedData, string salt, string password)
    {
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;

        var saltBytes = Convert.FromBase64String(salt);
        var key = new Rfc2898DeriveBytes(password, saltBytes, Iterations);
        aes.Key = key.GetBytes(aes.KeySize / 8);
        aes.IV = key.GetBytes(aes.BlockSize / 8);

        using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData));
        using var aesDecryptor = aes.CreateDecryptor();
        using var csDecrypt = new CryptoStream(msDecrypt, aesDecryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}