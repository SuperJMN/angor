using System.Text.RegularExpressions;

namespace SuppaWallet.Domain;

public sealed record WalletDescriptor
{
    public string Fingerprint { get; }
    public BitcoinNetwork Network { get; }
    public XPubCollection XPubs { get; }

    private WalletDescriptor(string fingerprint, BitcoinNetwork network, XPubCollection xpubs)
    {
        Fingerprint = fingerprint;
        Network = network;
        XPubs = xpubs;
    }

    public static WalletDescriptor Create(string fingerprint, BitcoinNetwork network, XPubCollection xpubs)
    {
        if (string.IsNullOrWhiteSpace(fingerprint) || !IsValidFingerprint(fingerprint))
            throw new DomainException("Invalid wallet fingerprint");

        return new WalletDescriptor(fingerprint, network, xpubs);
    }

    private static bool IsValidFingerprint(string fingerprint) =>
        Regex.IsMatch(fingerprint, "^[0-9a-f]{8}$", RegexOptions.IgnoreCase);

    // Para scanning de blockchain
    public IEnumerable<string> GetAllDerivationDescriptors()
    {
        foreach (var xpub in XPubs)
        {
            // Receiving addresses (0/*)
            yield return xpub.ToDescriptor(DerivationType.Receive);
            // Change addresses (1/*)
            yield return xpub.ToDescriptor(DerivationType.Change);
        }
    }
}