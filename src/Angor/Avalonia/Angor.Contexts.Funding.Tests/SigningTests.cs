using Angor.Contexts.Funding.Shared;
using Angor.Contexts.Funding.Tests.TestDoubles;
using Angor.Contexts.Integration.WalletFunding;
using Angor.Contexts.Wallet.Domain;
using Angor.Contexts.Wallet.Infrastructure.Impl;
using Angor.Contexts.Wallet.Infrastructure.Interfaces;
using Angor.Shared;
using Angor.Shared.Models;
using Angor.Shared.Networks;
using Angor.Shared.Services;
using Blockcore.NBitcoin;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NBitcoin;
using Nostr.Client.Client;
using Nostr.Client.Communicator;
using Nostr.Client.Keys;
using Serilog;
using Xunit.Abstractions;
using Key = NBitcoin.Key;
using Network = Blockcore.Networks.Network;

namespace Angor.Contexts.Funding.Tests;

public class SigningTests(ITestOutputHelper output)
{
    [Fact]
    public void Create_sign_service()
    {
        var signService =  CreateSignService();
    }

    [Fact]
    public async Task Post_investment_should_return_ok()
    {
        var sut = CreateSignService();

        var founderNostrPubKey = NostrPrivateKey.GenerateNew().DerivePublicKey().Hex;
        
        var founderPubKey = new Key().PubKey.ToHex();
        
        var keyIdenfier = new KeyIdentifier(WalletAppService.SingleWalletId.Value, founderPubKey);
            
        var result = await sut.PostInvestmentRequest2(keyIdenfier, "TEST", founderNostrPubKey);
        Assert.True(result.IsSuccess);
    }

    private ISignService CreateSignService()
    {
        var logger1 = new LoggerConfiguration()
            .WriteTo.TestOutput(output)
            .CreateLogger();


        var walletSensitiveDateProvider = new TestSensitiveWalletDataProvider(
            "print foil moment average quarter keep amateur shell tray roof acoustic where",
            ""
        );
        
        var serviceCollection = new ServiceCollection();
        FundingContextServices.Register(serviceCollection, logger1);
        serviceCollection.AddSingleton<ISeedwordsProvider, SeedwordsProvider>();
        serviceCollection.AddSingleton<ISensitiveWalletDataProvider>(walletSensitiveDateProvider);
        var provider = serviceCollection.BuildServiceProvider();

        return provider.GetRequiredService<ISignService>();
        
        var networkConfiguration = new TestNetworkConfiguration();
        var derivationOperations = new DerivationOperations(new HdOperations(), new NullLogger<DerivationOperations>(), networkConfiguration);
        
        var testingNostrSentiveData = new SensitiveNostrData(new TestingSeedwordsProvider("bla bla bla", "asdf"), derivationOperations, networkConfiguration);
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



internal class TestNetworkConfiguration : INetworkConfiguration
{
    public Network GetNetwork()
    {
        throw new NotImplementedException();
    }

    public void SetNetwork(Network network)
    {
        throw new NotImplementedException();
    }

    public string GetGenesisBlockHash()
    {
        throw new NotImplementedException();
    }

    public string GetNetworkNameFromGenesisBlockHash(string genesisBlockHash)
    {
        throw new NotImplementedException();
    }

    public List<SettingsUrl> GetDefaultIndexerUrls()
    {
        throw new NotImplementedException();
    }

    public List<SettingsUrl> GetDefaultRelayUrls()
    {
        throw new NotImplementedException();
    }

    public List<SettingsUrl> GetDefaultExplorerUrls()
    {
        throw new NotImplementedException();
    }

    public List<SettingsUrl> GetDefaultChatAppUrls()
    {
        throw new NotImplementedException();
    }

    public int GetAngorInvestFeePercentage { get; }
    public string GetAngorKey()
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, bool> GetDefaultFeatureFlags(string network)
    {
        throw new NotImplementedException();
    }
}

public class TestSensitiveWalletDataProvider : ISensitiveWalletDataProvider
{
    private readonly string seed;
    private readonly string passphrase;

    public TestSensitiveWalletDataProvider(string seed, string passphrase)
    {
        this.seed = seed;
        this.passphrase = passphrase;
    }

    public async Task<Result<(string seed, Maybe<string> passphrase)>> RequestSensitiveData(WalletId walletId)
    {
        if (walletId == WalletAppService.SingleWalletId)
        {
            return (seed, passphrase);
        }

        return Result.Failure<(string seed, Maybe<string> passphrase)>("Invalid id");
    }

    public void SetSensitiveData(WalletId id, (string seed, Maybe<string> passphrase) data)
    {
        throw new NotImplementedException();
    }
}