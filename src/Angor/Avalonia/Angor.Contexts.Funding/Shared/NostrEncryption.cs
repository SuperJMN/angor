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