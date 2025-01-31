namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class EncryptedWalletInfo
{
    // En claro
    public Guid WalletId { get; set; }
    public string WalletName { get; set; }

    // Cifrado
    public string EncryptedData { get; set; }
    public string Salt { get; set; }
}