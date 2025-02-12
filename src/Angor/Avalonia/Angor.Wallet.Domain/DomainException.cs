namespace SuppaWallet.Domain;

public class DomainException : Exception
{
    public DomainException(string invalidWalletFingerprint)
    {
    }
}