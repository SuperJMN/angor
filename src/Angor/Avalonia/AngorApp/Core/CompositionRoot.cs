using System.Linq;
using System.Threading.Tasks;
using Angor.UI.Model.Implementation;
using Angor.UI.Model.Implementation.Projects;
using AngorApp.Design;
using AngorApp.Sections;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Founder;
using AngorApp.Sections.Home;
using AngorApp.Sections.Portfolio;
using AngorApp.Sections.Shell;
using AngorApp.Sections.Wallet;
using AngorApp.Sections.Wallet.CreateAndRecover;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Create;
using AngorApp.Sections.Wallet.Unlock;
using AngorApp.Services;
using Avalonia.Controls.Notifications;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using ReactiveUI.Validation.Extensions;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Address;
using RefinedSuppaWalet.Infrastructure.Interfaces.Wallet;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Application.Services;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;
using RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;
using RefinedSuppaWallet.Infrastructure.Angor.Store;
using RefinedSuppaWallet.Intrastructure.Mempool.AddressManager;
using RefinedSuppaWallet.Intrastructure.Mempool.Repository;
using RefinedSuppaWallet.Intrastructure.Mempool.TransactionBroadcaster;
using RefinedSuppaWallet.Tests;
using Serilog;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Services;
using DefaultHttpClientFactory = Zafiro.Misc.DefaultHttpClientFactory;
using Network = NBitcoin.Network;
using Separator = AngorApp.Sections.Shell.Separator;

namespace AngorApp.Core;

public static class CompositionRoot
{
    public static MainViewModel CreateMainViewModel(Control control)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        var launcher = new LauncherService(topLevel!.Launcher);
        var uiServices = new UIServices(
            launcher,
            new DesktopDialog(),
            new NotificationService(new WindowNotificationManager(topLevel)
            {
                Position = NotificationPosition.BottomRight
            }));

        var dict = new Dictionary<WalletId, (Network, ExtKey)>();
        var dictFunc = () => dict;

        var walletAppService = WalletApplicationService(dictFunc, uiServices);
        var walletProvider = new WalletProvider(walletAppService);
        var walletFactory = new WalletFactory(new WalletBuilder(walletAppService), uiServices);

        MainViewModel mainViewModel = null!;

        var projectService = RealProjectService();

        IEnumerable<SectionBase> sections =
        [
            new Section("Home", () => new HomeSectionViewModel(walletProvider, uiServices, () => mainViewModel), "svg:/Assets/angor-icon.svg"),
            new Separator(),
            new Section("Wallet", () => new WalletSectionViewModel(walletFactory, walletAppService, walletProvider, uiServices, dictFunc), "fa-wallet"),
            new Section("Browse", () => new NavigationViewModel(navigator => new BrowseSectionViewModel(walletProvider, projectService, navigator, uiServices)), "fa-magnifying-glass"),
            new Section("Portfolio", () => new PortfolioSectionViewModel(), "fa-hand-holding-dollar"),
            new Section("Founder", () => new FounderSectionViewModel(projectService), "fa-money-bills"),
            new Separator(),
            new Section("Settings", () => null, "fa-gear"),
            new CommandSection("Angor Hub", ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(Constants.AngorHubUri)), "fa-magnifying-glass") { IsPrimary = false }
        ];

        mainViewModel = new MainViewModel(sections, uiServices);

        return mainViewModel;
    }

    private static WalletAppService WalletApplicationService(Func<Dictionary<WalletId, (Network, ExtKey)>> dict, UIServices uiServices)
    {
        var network = Network.TestNet;
        var bitcoinAddressTypeDetector = new NBitcoinAddressTypeDetector(network);
        var addressTypeDetector = bitcoinAddressTypeDetector;
        var addressManager = new AddressManager(network);
        var defaultHttpClientFactory = new DefaultHttpClientFactory();
        var mempoolUtxoRepository = new MempoolUtxoRepository(defaultHttpClientFactory, addressManager);
        var utxoSelector = new UtxoSelector();
        var transactionPreparer = new NBitcoinTransactionPreparer(mempoolUtxoRepository, network, addressManager, addressTypeDetector, new UtxoSelector());

        var mempoolTransactionBroadcaster = new MempoolTransactionBroadcaster(defaultHttpClientFactory);
        var bitcoinTransactionService = new BitcoinTransactionService(addressTypeDetector, mempoolUtxoRepository, utxoSelector, transactionPreparer, new NBitcoinTransactionSigner(dict), mempoolTransactionBroadcaster);

        var mempoolTransactionFetcher = new MempoolTransactionFetcher(Network.TestNet);
        var mempoolSpaceWalletService = new MempoolSpaceWalletService(Log.Logger, new MempoolAddressScanner(Network.TestNet), mempoolTransactionFetcher);
        var walletRepository = new WalletRepository(() => AngorWalletRepository.Create(mempoolSpaceWalletService, new FileStore("Angor")), new PassphraseProvider(uiServices));

        return new WalletAppService(bitcoinTransactionService, walletRepository);
    }

    private static ProjectService RealProjectService()
    {
        var loggerFactory = LoggerConfig.CreateFactory();
        return new ProjectService(DependencyFactory.GetIndexerService(loggerFactory), DependencyFactory.GetRelayService(loggerFactory));
    }
}

internal class PassphraseProvider : IPassphraseProvider
{
    private readonly UIServices services;

    public PassphraseProvider(UIServices services)
    {
        this.services = services;
    }

    public async Task<Maybe<string>> Provide(WalletId id)
    {
        var passphraseRequestViewModel = new PassphraseRequestViewModel();
        await services.Dialog.Show(passphraseRequestViewModel, "Unlock wallet", passphraseRequestViewModel.IsValid());
        return passphraseRequestViewModel.Passphrase;
    }
}

internal class WalletRepository : IWalletRepository
{
    private readonly Func<Task<IProtectedWalletRepository>> inner;
    private readonly IPassphraseProvider provider;
    private readonly Dictionary<WalletId, Wallet> unlockedWallets = new();

    public WalletRepository(Func<Task<IProtectedWalletRepository>> inner, IPassphraseProvider provider)
    {
        this.inner = inner;
        this.provider = provider;
    }

    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return await (await inner()).ListWallets();
    }

    public Task<Maybe<Wallet>> Get(WalletId id)
    {
        return unlockedWallets.TryFind(id).Or(() => provider.Provide(id).Bind(async passphrase => await (await inner()).Get(id, passphrase)));
    }

    public async Task<Result<Wallet>> ImportWallet(WalletId walletId, string walletName, WalletDescriptor descriptor, string passphrase)
    {
        return await (await inner()).ImportWallet(walletId, walletName, descriptor, passphrase);
    }
}