using Angor.Client;
using Angor.Client.Storage;
using Angor.Shared;
using Angor.Shared.Models;
using Angor.Shared.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Angor.Test;

public class Suppa
{
    [Fact]
    public async Task GetProjects()
    {
        var networkConfiguration = new NetworkConfiguration();
        var networkConfig = networkConfiguration;
        var httpClient = new HttpClient();
        var indexer = new IndexerService(networkConfig, httpClient, new NetworkService(new TestStorage(), httpClient, new NullLogger<NetworkService>(), networkConfiguration));
        var proj = await indexer.GetProjectsAsync(0, 21);
    }
}

public class TestStorage : INetworkStorage
{
    public SettingsInfo GetSettings()
    {
        return new SettingsInfo()
        {
            Indexers = new List<SettingsUrl>()
            {
                new SettingsUrl()
                {
                    Name = "test",
                    IsPrimary = true,
                    Url = "https://tbtc.indexer.angor.io",
                }
            }
        };
    }

    public void SetSettings(SettingsInfo settingsInfo)
    {
        throw new NotImplementedException();
    }

    public void SetNetwork(string network)
    {
        throw new NotImplementedException();
    }

    public string GetNetwork()
    {
        throw new NotImplementedException();
    }
}