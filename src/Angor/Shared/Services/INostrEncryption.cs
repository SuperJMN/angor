using Nostr.Client.Messages;

public interface INostrEncryption
{
    NostrEvent Encrypt(NostrEvent ev, string localPrivateKey, string remotePublicKey);
}