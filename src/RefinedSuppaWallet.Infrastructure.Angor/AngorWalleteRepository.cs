using CSharpFunctionalExtensions;
using NBitcoin;
using ScriptType = RefinedSuppaWallet.Domain.ScriptType;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class WalletData
{
    // Se utiliza para mostrar la información de solo lectura
    public string? DescriptorJson { get; set; }

    // Indica si la wallet fue creada con passphrase
    public bool RequiresPassphrase { get; set; }

    // Información sensible para firmar (por ejemplo, las seedwords)
    public string? SeedWords { get; set; }
}

public interface IWalletEncryption
{
    Task<Result<WalletData>> Decrypt(EncryptedWallet wallet, string encryptionKey);
    Task<EncryptedWallet> Encrypt(WalletData walletData, string encryptionKey, string name, Guid id);
}

public record WalletDescriptorDto(
    string Fingerprint,
    string Network, // Representamos la red como string para simplificar la serialización.
    IEnumerable<XPubDto> XPubs
);

public record XPubDto(
    string Value,
    ScriptType ScriptType,
    DerivationPathDto Path
);

public record DerivationPathDto(
    uint Purpose,
    uint CoinType,
    uint Account
);

public class EncryptedWallet
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EncryptedData { get; set; }
    public string Salt { get; set; }
    public string IV { get; set; }
}