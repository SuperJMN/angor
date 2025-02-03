using Angor.UI.Model.Implementation;
using Angor.UI.Model.Implementation.Projects;
using AngorApp.Sections;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Founder;
using AngorApp.Sections.Home;
using AngorApp.Sections.Portfolio;
using AngorApp.Sections.Shell;
using AngorApp.Sections.Wallet;
using AngorApp.Services;
using Avalonia.Controls.Notifications;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Address;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;
using RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;
using RefinedSuppaWallet.Infrastructure.Angor.Store;
using RefinedSuppaWallet.Intrastructure.Mempool.AddressManager;
using RefinedSuppaWallet.Intrastructure.Mempool.Repository;
using RefinedSuppaWallet.Intrastructure.Mempool.TransactionBroadcaster;
using Serilog;
using Serilog.Core;
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

        var passphraseProvider = new PassphraseProvider(uiServices);
        var walletAppService = WalletApplicationService(passphraseProvider);

        var walletProvider = new WalletProvider(walletAppService);
        var walletFactory = new WalletFactory(new WalletBuilder(walletAppService, passphraseProvider), uiServices);

        MainViewModel mainViewModel = null!;

        var projectService = RealProjectService();

        IEnumerable<SectionBase> sections =
        [
            new Section("Home", () => new HomeSectionViewModel(walletProvider, uiServices, () => mainViewModel), "svg:/Assets/angor-icon.svg"),
            new Separator(),
            new Section("Wallet", () => new WalletSectionViewModel(walletFactory, walletProvider, uiServices, walletAppService, passphraseProvider), "fa-wallet"),
            new Section("Browse", () => new NavigationViewModel(navigator => new BrowseSectionViewModel(walletProvider, projectService, navigator, uiServices, walletAppService, passphraseProvider)), "fa-magnifying-glass"),
            new Section("Portfolio", () => new PortfolioSectionViewModel(), "fa-hand-holding-dollar"),
            new Section("Founder", () => new FounderSectionViewModel(projectService), "fa-money-bills"),
            new Separator(),
            new Section("Settings", () => null, "fa-gear"),
            new CommandSection("Angor Hub", ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(Constants.AngorHubUri)), "fa-magnifying-glass") { IsPrimary = false }
        ];

        mainViewModel = new MainViewModel(sections, uiServices);

        return mainViewModel;
    }

    private static WalletAppService WalletApplicationService(PassphraseProvider passphraseProvider)
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
        var mempoolTransactionFetcher = new MempoolTransactionFetcher(Network.TestNet);
        var walletRepository = new AngorWalleteRepository(new FileStore("Angor"), passphraseProvider);
        var bitcoinTransactionService = new BitcoinTransactionService(addressTypeDetector, mempoolUtxoRepository, utxoSelector, transactionPreparer, new NBitcoinTransactionSigner(walletRepository, passphraseProvider), mempoolTransactionBroadcaster);
        var walletTransactionService = new MempoolSpaceWalletService(Logger.None, new MempoolAddressScanner(Network.TestNet), mempoolTransactionFetcher);
        var blockchainService = new BlockchainService(mempoolUtxoRepository, bitcoinTransactionService, walletTransactionService, mempoolTransactionBroadcaster);
        return new WalletAppService(walletRepository, blockchainService, new TransactionSigner());
    }

    private static ProjectService RealProjectService()
    {
        var loggerFactory = LoggerConfig.CreateFactory();
        return new ProjectService(DependencyFactory.GetIndexerService(loggerFactory), DependencyFactory.GetRelayService(loggerFactory));
    }
}