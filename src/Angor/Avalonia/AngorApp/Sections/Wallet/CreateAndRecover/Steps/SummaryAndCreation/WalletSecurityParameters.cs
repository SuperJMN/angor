using Angor.UI.Model;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public class WalletSecurityParameters(Maybe<string> passphrase, SeedWords seedwords, string encryptionKey)
{
    public Maybe<string> Passphrase { get; private set; } = passphrase;
    public SeedWords Seedwords { get; private set; } = seedwords;
    public string EncryptionKey { get; private set; } = encryptionKey;
}