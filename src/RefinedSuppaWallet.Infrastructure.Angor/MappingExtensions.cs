using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public static class MappingExtensions
{
    // Mapea del dominio al DTO
    public static WalletDescriptorDto ToDto(this WalletDescriptor descriptor) =>
        new WalletDescriptorDto(
            descriptor.Fingerprint,
            descriptor.Network.ToString(), // Asumimos que Network tiene un ToString() adecuado.
            descriptor.XPubs.Select(x => x.ToDto())
        );

    public static XPubDto ToDto(this XPub xpub) =>
        new XPubDto(
            xpub.Value,
            xpub.ScriptType,
            new DerivationPathDto(xpub.Path.Purpose, xpub.Path.CoinType, xpub.Path.Account)
        );

    // Mapea del DTO al dominio
    public static Result<WalletDescriptor> ToDomain(this WalletDescriptorDto dto)
    {
        // Reconstruimos los XPub a partir del DTO usando LINQ, sin bucles imperativos.
        var xpubList = dto.XPubs.Select(x => x.ToDomain()).ToList();

        // Suponemos que tenemos, por lo menos, los xpub necesarios (por ejemplo, SegWit y Taproot)
        var segwitXpub = xpubList.FirstOrDefault(x => x.ScriptType == ScriptType.SegWit);
        var taprootXpub = xpubList.FirstOrDefault(x => x.ScriptType == ScriptType.Taproot);
        if (segwitXpub is null || taprootXpub is null)
            return Result.Failure<WalletDescriptor>("The required XPubs are missing to create a Wallet Descriptor");

        var xpubCollection = XPubCollection.Create(segwitXpub, taprootXpub);

        // Convertimos el string de red al objeto de dominio. Asumimos que BitcoinNetwork tiene un método Parse.
        if (!Enum.TryParse<BitcoinNetwork>(dto.Network, out var network))
        {
            return Result.Failure<WalletDescriptor>($"Invalid network found for Wallet Descriptor: {dto.Network}");
        }

        return WalletDescriptor.Create(dto.Fingerprint, network, xpubCollection);
    }

    public static XPub ToDomain(this XPubDto dto)
    {
        var path = DerivationPath.Create(dto.Path.Purpose, dto.Path.CoinType, dto.Path.Account);
        return XPub.Create(dto.Value, dto.ScriptType, path);
    }
}