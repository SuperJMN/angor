using Angor.Shared.Services;
using CSharpFunctionalExtensions;
using Nostr.Client.Keys;
using Nostr.Client.Messages;

namespace Angor.Contexts.Funding.Shared;

public class NostrEncryption : INostrEncryption
{
    public NostrEvent Encrypt(NostrEvent ev, string localPrivateKey, string remotePublicKey)
    {
        var privateKey = NostrPrivateKey.FromHex(localPrivateKey);
        var nostrPubKey = NostrPublicKey.FromHex(remotePublicKey);

        return ev.Encrypt(privateKey, nostrPubKey);
    }
}

public class SensitiveNostrData : ISensitiveNostrData
{
    private readonly ISeedwordsProvider seedwordsProvider;

    public SensitiveNostrData(ISeedwordsProvider seedwordsProvider)
    {
        this.seedwordsProvider = seedwordsProvider;
    }
    
    
    public Result<string> GetNostrPrivateKey(KeyIdentifier keyIdentifier)
    {
        return NostrPrivateKey.GenerateNew().Hex;
    }
}