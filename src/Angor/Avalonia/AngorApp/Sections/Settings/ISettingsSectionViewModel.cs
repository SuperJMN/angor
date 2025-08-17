using System.Collections.ObjectModel;
using Angor.Shared.Models;
using ReactiveUI;

namespace AngorApp.Sections.Settings;

internal interface ISettingsSectionViewModel : IDisposable
{
    ObservableCollection<SettingsUrl> Explorers { get; }
    ObservableCollection<SettingsUrl> Indexers { get; }
    ObservableCollection<SettingsUrl> Relays { get; }
    IReadOnlyList<string> Networks { get; }
    string Network { get; set; }
    string NewExplorer { get; set; }
    string NewIndexer { get; set; }
    string NewRelay { get; set; }
    ReactiveCommand<Unit, Unit> AddExplorer { get; }
    ReactiveCommand<SettingsUrl, Unit> RemoveExplorer { get; }
    ReactiveCommand<SettingsUrl, Unit> SetPrimaryExplorer { get; }
    ReactiveCommand<Unit, Unit> AddIndexer { get; }
    ReactiveCommand<SettingsUrl, Unit> RemoveIndexer { get; }
    ReactiveCommand<SettingsUrl, Unit> SetPrimaryIndexer { get; }
    ReactiveCommand<Unit, Unit> AddRelay { get; }
    ReactiveCommand<SettingsUrl, Unit> RemoveRelay { get; }
}

internal class SettingsSectionViewModelDesign : ISettingsSectionViewModel
{
    public SettingsSectionViewModelDesign()
    {
        Explorers = new ObservableCollection<SettingsUrl>
        {
            new() { Url = "https://explorer.angor.io", IsPrimary = true }
        };
        Indexers = new ObservableCollection<SettingsUrl>
        {
            new() { Url = "https://indexer.angor.io", IsPrimary = true }
        };
        Relays = new ObservableCollection<SettingsUrl>
        {
            new() { Url = "wss://relay.angor.io" }
        };
    }

    public ObservableCollection<SettingsUrl> Explorers { get; }
    public ObservableCollection<SettingsUrl> Indexers { get; }
    public ObservableCollection<SettingsUrl> Relays { get; }
    public IReadOnlyList<string> Networks { get; } = new[] { "Angornet", "Mainnet" };
    public string Network { get; set; } = "Angornet";
    public string NewExplorer { get; set; } = string.Empty;
    public string NewIndexer { get; set; } = string.Empty;
    public string NewRelay { get; set; } = string.Empty;
    public ReactiveCommand<Unit, Unit> AddExplorer { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<SettingsUrl, Unit> RemoveExplorer { get; } = ReactiveCommand.Create<SettingsUrl>(_ => { });
    public ReactiveCommand<SettingsUrl, Unit> SetPrimaryExplorer { get; } = ReactiveCommand.Create<SettingsUrl>(_ => { });
    public ReactiveCommand<Unit, Unit> AddIndexer { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<SettingsUrl, Unit> RemoveIndexer { get; } = ReactiveCommand.Create<SettingsUrl>(_ => { });
    public ReactiveCommand<SettingsUrl, Unit> SetPrimaryIndexer { get; } = ReactiveCommand.Create<SettingsUrl>(_ => { });
    public ReactiveCommand<Unit, Unit> AddRelay { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<SettingsUrl, Unit> RemoveRelay { get; } = ReactiveCommand.Create<SettingsUrl>(_ => { });
    public void Dispose() { }
}
