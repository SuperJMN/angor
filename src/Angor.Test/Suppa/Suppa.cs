using Angor.Shared;
using Angor.Shared.Models;

namespace Angor.Test.Suppa;

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
            },
            Relays = new List<SettingsUrl>()
            {
                new SettingsUrl()
                {
                    Name = "relay",
                    IsPrimary = true,
                    Url = "wss://relay.angor.io",
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