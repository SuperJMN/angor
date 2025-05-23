using Angor.Contexts.Funding.Shared;
using Angor.Contexts.Funding.Tests.TestDoubles;
using Angor.Shared;
using Angor.Shared.Models;
using Angor.Shared.Networks;
using Angor.Shared.Services;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
using Nostr.Client.Keys;
using Serilog;

namespace Angor.Contexts.Funding.Tests;

public class SigningTests
{
    [Fact]
    public async Task Post_investment_should_return_ok()
    {
        var sut = CreateSignService();

        var founderNostrPubKey = NostrPrivateKey.GenerateNew().DerivePublicKey().Hex;
        var founderPubKey = NostrPrivateKey.GenerateNew().DerivePublicKey().Hex;

        var keyIdenfier = new KeyIdentifier(Guid.Empty, founderPubKey);

        var result = await sut.PostInvestmentRequest2(keyIdenfier, "TEST", founderNostrPubKey);
        Assert.True(result.IsSuccess);
    }

    private static SignService CreateSignService()
    {
        var testingNostrSentiveData = new SensitiveNostrData(new TestingSeedwordsProvider("bla bla bla", "asdf"));
        var serializer = new Serializer();
        var testingNotrEncription = new NostrEncryption();
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

        var networkService = new NetworkService(mockNetworkStorage.Object, new HttpClient { BaseAddress = new Uri("wss://relay.angor.io") }, new NullLogger<NetworkService>(), mockNetworkConfiguration.Object);
        var subscriptionsHanding = new RelaySubscriptionsHandling(new NullLogger<RelaySubscriptionsHandling>(), communicationFactory, networkService);

        var nostrWebsocketCommunicator = new NostrWebsocketCommunicator(new Uri("ws://relay.angor.io"));
        nostrWebsocketCommunicator.Start().Wait();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddSerilog());
        var logger = loggerFactory.CreateLogger<NostrWebsocketClient>();
        var nostrSmart = new NostrService(new NostrWebsocketClient(nostrWebsocketCommunicator, logger));
        var sut = new SignService(testingNostrSentiveData, serializer, testingNotrEncription, communicationFactory, networkService, subscriptionsHanding, nostrSmart);
        return sut;
    }
}