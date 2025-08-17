using System.Collections.ObjectModel;
using Angor.Contexts.Wallet.Infrastructure.Impl;
using Angor.Contexts.Wallet.Infrastructure.Interfaces;
using Angor.Shared;
using Angor.Shared.Models;
using System.Linq;
using AngorApp.UI.Services;
using ReactiveUI;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Sections.Settings;

public partial class SettingsSectionViewModel : ReactiveObject, ISettingsSectionViewModel, IDisposable
{
    private readonly INetworkStorage networkStorage;
    private readonly IWalletStore walletStore;
    private readonly UIServices uiServices;
    private string currentNetwork;

    private string network;
    private string newExplorer;
    private string newIndexer;
    private string newRelay;

    private readonly IDisposable networkChanged;

    public SettingsSectionViewModel(INetworkStorage networkStorage, IWalletStore walletStore, UIServices uiServices)
    {
        this.networkStorage = networkStorage;
        this.walletStore = walletStore;
        this.uiServices = uiServices;

        var settings = networkStorage.GetSettings();
        Explorers = new ObservableCollection<SettingsUrl>(settings.Explorers);
        Indexers = new ObservableCollection<SettingsUrl>(settings.Indexers);
        Relays = new ObservableCollection<SettingsUrl>(settings.Relays);

        currentNetwork = networkStorage.GetNetwork();
        Network = currentNetwork;

        AddExplorer = ReactiveCommand.Create(AddExplorerImpl, this.WhenAnyValue(x => x.NewExplorer, url => !string.IsNullOrWhiteSpace(url)));
        RemoveExplorer = ReactiveCommand.Create<SettingsUrl>(RemoveExplorerImpl);
        SetPrimaryExplorer = ReactiveCommand.Create<SettingsUrl>(SetPrimaryExplorerImpl);

        AddIndexer = ReactiveCommand.Create(AddIndexerImpl, this.WhenAnyValue(x => x.NewIndexer, url => !string.IsNullOrWhiteSpace(url)));
        RemoveIndexer = ReactiveCommand.Create<SettingsUrl>(RemoveIndexerImpl);
        SetPrimaryIndexer = ReactiveCommand.Create<SettingsUrl>(SetPrimaryIndexerImpl);

        AddRelay = ReactiveCommand.Create(AddRelayImpl, this.WhenAnyValue(x => x.NewRelay, url => !string.IsNullOrWhiteSpace(url)));
        RemoveRelay = ReactiveCommand.Create<SettingsUrl>(RemoveRelayImpl);

        networkChanged = this.WhenAnyValue(x => x.Network)
            .Skip(1)
            .SelectMany(async n => (n, await uiServices.Dialog.ShowConfirmation("Change network?", "Changing network will delete the current wallet")))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(t => t.Item2.Match(
                confirmed =>
                {
                    if (confirmed)
                    {
                        networkStorage.SetNetwork(t.n);
                        walletStore.SaveAll(Enumerable.Empty<EncryptedWallet>());
                        currentNetwork = t.n;
                    }
                    else
                    {
                        Network = currentNetwork;
                    }
                    return Unit.Default;
                },
                () =>
                {
                    Network = currentNetwork;
                    return Unit.Default;
                }));

        Save = ReactiveCommand.Create(SaveSettings);

        this.WhenAnyObservable(x => x.AddExplorer, x => x.RemoveExplorer, x => x.SetPrimaryExplorer,
            x => x.AddIndexer, x => x.RemoveIndexer, x => x.SetPrimaryIndexer,
            x => x.AddRelay, x => x.RemoveRelay)
            .InvokeCommand(Save);
    }

    public ObservableCollection<SettingsUrl> Explorers { get; }
    public ObservableCollection<SettingsUrl> Indexers { get; }
    public ObservableCollection<SettingsUrl> Relays { get; }

    public IReadOnlyList<string> Networks { get; } = new[] { "Angornet", "Mainnet" };

    public ReactiveCommand<Unit, Unit> AddExplorer { get; }
    public ReactiveCommand<SettingsUrl, Unit> RemoveExplorer { get; }
    public ReactiveCommand<SettingsUrl, Unit> SetPrimaryExplorer { get; }

    public ReactiveCommand<Unit, Unit> AddIndexer { get; }
    public ReactiveCommand<SettingsUrl, Unit> RemoveIndexer { get; }
    public ReactiveCommand<SettingsUrl, Unit> SetPrimaryIndexer { get; }

    public ReactiveCommand<Unit, Unit> AddRelay { get; }
    public ReactiveCommand<SettingsUrl, Unit> RemoveRelay { get; }

    public ReactiveCommand<Unit, Unit> Save { get; }

    public string Network
    {
        get => network;
        set => this.RaiseAndSetIfChanged(ref network, value);
    }

    public string NewExplorer
    {
        get => newExplorer;
        set => this.RaiseAndSetIfChanged(ref newExplorer, value);
    }

    public string NewIndexer
    {
        get => newIndexer;
        set => this.RaiseAndSetIfChanged(ref newIndexer, value);
    }

    public string NewRelay
    {
        get => newRelay;
        set => this.RaiseAndSetIfChanged(ref newRelay, value);
    }

    void AddExplorerImpl()
    {
        Explorers.Add(new SettingsUrl { Url = NewExplorer, IsPrimary = Explorers.Count == 0 });
        NewExplorer = string.Empty;
        Refresh(Explorers);
    }

    void RemoveExplorerImpl(SettingsUrl url)
    {
        var wasPrimary = url.IsPrimary;
        Explorers.Remove(url);
        if (wasPrimary && Explorers.Count > 0)
        {
            Explorers[0].IsPrimary = true;
        }
        Refresh(Explorers);
    }

    void SetPrimaryExplorerImpl(SettingsUrl url)
    {
        foreach (var e in Explorers)
        {
            e.IsPrimary = false;
        }
        url.IsPrimary = true;
        Refresh(Explorers);
    }

    void AddIndexerImpl()
    {
        Indexers.Add(new SettingsUrl { Url = NewIndexer, IsPrimary = Indexers.Count == 0 });
        NewIndexer = string.Empty;
        Refresh(Indexers);
    }

    void RemoveIndexerImpl(SettingsUrl url)
    {
        var wasPrimary = url.IsPrimary;
        Indexers.Remove(url);
        if (wasPrimary && Indexers.Count > 0)
        {
            Indexers[0].IsPrimary = true;
        }
        Refresh(Indexers);
    }

    void SetPrimaryIndexerImpl(SettingsUrl url)
    {
        foreach (var e in Indexers)
        {
            e.IsPrimary = false;
        }
        url.IsPrimary = true;
        Refresh(Indexers);
    }

    void AddRelayImpl()
    {
        Relays.Add(new SettingsUrl { Url = NewRelay });
        NewRelay = string.Empty;
        Refresh(Relays);
    }

    void RemoveRelayImpl(SettingsUrl url)
    {
        Relays.Remove(url);
        Refresh(Relays);
    }

    void Refresh<T>(ObservableCollection<T> collection)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            var item = collection[i];
            collection[i] = item;
        }
    }

    void SaveSettings()
    {
        var info = new SettingsInfo
        {
            Explorers = new List<SettingsUrl>(Explorers),
            Indexers = new List<SettingsUrl>(Indexers),
            Relays = new List<SettingsUrl>(Relays)
        };
        networkStorage.SetSettings(info);
    }

    public void Dispose()
    {
        AddExplorer.Dispose();
        RemoveExplorer.Dispose();
        SetPrimaryExplorer.Dispose();
        AddIndexer.Dispose();
        RemoveIndexer.Dispose();
        SetPrimaryIndexer.Dispose();
        AddRelay.Dispose();
        RemoveRelay.Dispose();
        Save.Dispose();
        networkChanged.Dispose();
    }
}

