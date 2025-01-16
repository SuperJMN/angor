using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Angor.Shared.Models;
using Angor.Shared.Services;
using Angor.Test.Suppa;
using DynamicData;
using Microsoft.Extensions.Logging;
using Nostr.Client.Client;
using Xunit.Abstractions;

namespace Angor.Model.Implementation.Tests;

public class ProjectServiceTests
{
    private readonly ITestOutputHelper output;
    private readonly ILoggerFactory loggerFactory;

    public ProjectServiceTests(ITestOutputHelper output)
    {
        this.output = output;
        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddXUnitLogger(this.output);
        });
    }

    [Fact]
    public async Task GetProjectsFromService()
    {
        var service = new ProjectService(GetIndexerService(), GetRelayService());

        var projectsList = await service.Connect()
            .ToCollection()
            .Where(projects => projects.Any())
            .Take(1) 
            .ToTask();

        Assert.NotEmpty(projectsList);
    }
    
    [Fact]
    public async Task Relay()
    {
        var relay = GetRelayService();
        relay.LookupProjectsInfoByEventIds<ProjectInfo>(projectInfo => { }, () => { }, new string[] { "fa8e06bf2d44a53a14e65ee2a886f82caccacd7b77a16800c681c1f59b98babb" });
        await Task.Delay(20000);
    }
    
    private IRelayService GetRelayService()
    {
        var relaySubscriptionsLogger = loggerFactory.CreateLogger<RelaySubscriptionsHandling>();
        var relayServiceLogger = loggerFactory.CreateLogger<RelayService>();
        var clientLogger = loggerFactory.CreateLogger<NostrWebsocketClient>();
        var networkServiceLogger = loggerFactory.CreateLogger<NetworkService>();
        var nostrCommunicationFactoryLogger = loggerFactory.CreateLogger<NostrCommunicationFactory>();

        var networkConfiguration = new NetworkConfiguration();
        var httpClient = new HttpClient();
        var networkService = new NetworkService(new TestStorage(), httpClient, networkServiceLogger, networkConfiguration);
        var nostrCommunicationFactory = new NostrCommunicationFactory(clientLogger, nostrCommunicationFactoryLogger);
        var relaySubscriptionsHandling = new RelaySubscriptionsHandling(relaySubscriptionsLogger, nostrCommunicationFactory, networkService);
        
        var relay = new RelayService(relayServiceLogger, nostrCommunicationFactory, networkService, relaySubscriptionsHandling, new Serializer());
        return relay;
    }

    private IIndexerService GetIndexerService()
    {
        var networkServiceLogger = loggerFactory.CreateLogger<NetworkService>();
        var networkConfiguration = new NetworkConfiguration();
        var networkConfig = networkConfiguration;
        var httpClient = new HttpClient();
        var networkService = new NetworkService(new TestStorage(), httpClient, networkServiceLogger, networkConfiguration);
        
        return new IndexerService(networkConfig, httpClient, networkService);
    }
}