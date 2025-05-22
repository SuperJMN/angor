using System.Reactive.Linq;
using Angor.Shared;
using Angor.Shared.Models;
using Angor.Shared.Networks;
using Angor.Shared.Services;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NBitcoin.Secp256k1;
using Nostr.Client.Client;
using Nostr.Client.Keys;
using Nostr.Client.Messages;
using Nostr.Client.Messages.Direct;
using Nostr.Client.Responses;

namespace Angor.Test;

public class SigningTests
{
    [Fact]
    public async Task Test_something()
    {
        var testingNostrSentiveData = new TestingNostrSentiveData();
        var serializer = new Serializer();
        var testingNotrEncription = new TestingNostrEncription();
        var communicationFactory = new NostrCommunicationFactory(new NullLogger<NostrWebsocketClient>(), new NullLogger<NostrCommunicationFactory>());
        var mockNetworkConfiguration = new Mock<INetworkConfiguration>();
        var mockNetworkStorage = new Mock<INetworkStorage>();
        mockNetworkStorage.Setup(ns => ns.GetSettings()).Returns(new SettingsInfo
        {
            Relays = new List<SettingsUrl>
            {
                new() { Name = "", Url = "wss://relay.angor.io", IsPrimary = true },
                //new() { Name = "", Url = "wss://relay2.angor.io", IsPrimary = true },
            },
        });
        mockNetworkConfiguration.Setup(nc => nc.GetAngorKey()).Returns("dummyAngorKey");
        mockNetworkConfiguration.Setup(nc => nc.GetNetwork()).Returns(Networks.Bitcoin.Testnet);

        var networkService = new NetworkService(mockNetworkStorage.Object, new HttpClient { BaseAddress = new Uri("https://angor.io") }, new NullLogger<NetworkService>(), mockNetworkConfiguration.Object);
        var subscriptionsHanding = new RelaySubscriptionsHandling(new NullLogger<RelaySubscriptionsHandling>(), communicationFactory, networkService);

        var sut = new SignService(testingNostrSentiveData, serializer, testingNotrEncription, communicationFactory, networkService, subscriptionsHanding);

        var founderNostrPubKey = NostrPrivateKey.GenerateNew().DerivePublicKey().Hex;
        var founderPubKey = NostrPrivateKey.GenerateNew().DerivePublicKey().Hex;

        var keyIdenfier = new KeyIdentifier(Guid.Empty, founderPubKey);

        sut.PostInvestmentRequest2(keyIdenfier, "TEST", founderNostrPubKey)
            .Timeout(TimeSpan.FromSeconds(2))
            .Subscribe(response => {});

        await Task.Delay(20000);
    }
}

// TODO: Consider moving this as the real implementation
public class TestingNostrEncription : INostrEncryption
{
    public NostrEvent Encrypt(NostrEvent ev, string localPrivateKey, string remotePublicKey)
    {
        var privateKey = NostrPrivateKey.FromHex(localPrivateKey);
        var nostrPubKey = NostrPublicKey.FromHex(remotePublicKey);

        return ev.Encrypt(privateKey, nostrPubKey);
    }
}

public class TestingNostrSentiveData : ISensitiveNostrData
{
    public Result<string> GetNostrPrivateKey(KeyIdentifier keyIdentifier)
    {
        return NostrPrivateKey.GenerateNew().Hex;
    }
}