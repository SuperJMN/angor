using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Angor.Client;
using Angor.Shared.Models;
using Angor.Shared.Services;
using Blockcore.Configuration.Logging;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog.Extensions.Logging;
using Nostr.Client.Client;
using SuperServices;
using Xunit.Abstractions;

namespace Angor.Test.Suppa;

public class SuppaTests
{
    private readonly ITestOutputHelper _output;
    private readonly ILoggerFactory _loggerFactory;

    public SuppaTests(ITestOutputHelper output)
    {
        _output = output;
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddXUnitLogger(_output);
        });
    }

    [Fact]
    public async Task GetProjectsFromService()
    {
        var service = new ProjectService(GetIndexerService(), GetRelayService());

        // Conectamos el servicio y convertimos la secuencia a una colección,
        // esperando el primer valor que cumpla 'hay elementos en la lista'.
        var projectsList = await service.Connect()
            .ToCollection()
            .Where(projects => projects.Any())
            .Take(1) // tomamos solo la primera emisión que cumpla la condición
            .ToTask();

        // ¡Ahora sí, podemos verificar!
        Assert.NotEmpty(projectsList);
    }
    
    [Fact]
    public async Task Relay()
    {
        var relay = GetRelayService();
        relay.LookupProjectsInfoByEventIds<ProjectInfo>(projectInfo => { }, () => { }, new string[] { "fa8e06bf2d44a53a14e65ee2a886f82caccacd7b77a16800c681c1f59b98babb" });
        await Task.Delay(20000);
    }
    
    [Fact]
    public async Task GetProjects()
    {
        var networkConfiguration = new NetworkConfiguration();
        var networkConfig = networkConfiguration;
        var httpClient = new HttpClient();
        var networkService = new NetworkService(new TestStorage(), httpClient, new NullLogger<NetworkService>(), networkConfiguration);
        var indexer = new IndexerService(networkConfig, httpClient, networkService);
        var proj = await indexer.GetProjectsAsync(0, 21);

        var a = new Logger<NostrWebsocketClient>(new NLogLoggerFactory());
        var clientLogger = new Logger<NostrWebsocketClient>(new NLogLoggerFactory());

        var nostrCommunicationFactory = new NostrCommunicationFactory(clientLogger, new Logger<NostrCommunicationFactory>(new ExtendedLoggerFactory()));
        var logger = new Logger<RelaySubscriptionsHandling>(new ExtendedLoggerFactory());
        var relaySubscriptionsHandling = new RelaySubscriptionsHandling(logger, nostrCommunicationFactory, networkService);
        var relay = new RelayService(new NullLogger<RelayService>(), nostrCommunicationFactory, networkService, relaySubscriptionsHandling, new Serializer());
        relay.LookupProjectsInfoByEventIds<ProjectInfo>(obj => { }, () => { }, new[] { proj.First().NostrEventId });
    }

    private IRelayService GetRelayService()
    {
        var relaySubscriptionsLogger = _loggerFactory.CreateLogger<RelaySubscriptionsHandling>();
        var relayServiceLogger = _loggerFactory.CreateLogger<RelayService>();
        var clientLogger = _loggerFactory.CreateLogger<NostrWebsocketClient>();
        var networkServiceLogger = _loggerFactory.CreateLogger<NetworkService>();
        var nostrCommunicationFactoryLogger = _loggerFactory.CreateLogger<NostrCommunicationFactory>();

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
        var networkServiceLogger = _loggerFactory.CreateLogger<NetworkService>();
        var networkConfiguration = new NetworkConfiguration();
        var networkConfig = networkConfiguration;
        var httpClient = new HttpClient();
        var networkService = new NetworkService(new TestStorage(), httpClient, networkServiceLogger, networkConfiguration);
        
        return new IndexerService(networkConfig, httpClient, networkService);
    }
}