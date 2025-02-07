using System.Threading.Tasks;
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
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Address;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;
using RefinedSuppaWallet.Infrastructure.Angor.Store;
using RefinedSuppaWallet.Intrastructure.Mempool.AddressManager;
using RefinedSuppaWallet.Intrastructure.Mempool.Repository;
using RefinedSuppaWallet.Intrastructure.Mempool.TransactionBroadcaster;
using Serilog.Core;
using Zafiro.Avalonia.Dialogs;
using Zafiro.Avalonia.Services;
using DefaultHttpClientFactory = Zafiro.Misc.DefaultHttpClientFactory;
using Network = NBitcoin.Network;
using Separator = AngorApp.Sections.Shell.Separator;

namespace AngorApp.Core;

public static class CompositionRoot
{
    public static async Task<MainViewModel> CreateMainViewModel(Control control)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        var launcher = new LauncherService(topLevel!.Launcher);
        var uiServices = new UIServices(
            launcher,
            new DesktopDialog(),
            new NotificationService(new WindowNotificationManager(topLevel)
            {
                Position = NotificationPosition.BottomRight
            }),
            new ActiveWallet());

        var walletUnlocker = new WalletUnlockHandler(uiServices);
        var walletRepository = new AngorWalletDataRepository(new FileStore("Angor"), walletUnlocker, new AesWalletEncryption(), new PassphraseProvider(uiServices), new EncryptionKeyProvider(uiServices));
        var walletAppService = WalletApplicationService(walletRepository);
        
        var walletBuilder = new WalletBuilder(walletAppService, walletUnlocker);
        var walletFactory = new WalletFactory(walletBuilder, uiServices, walletAppService, walletUnlocker);

        MainViewModel mainViewModel = null!;

        var projectService = RealProjectService();

        IEnumerable<SectionBase> sections =
        [
            new Section("Home", () => new HomeSectionViewModel(walletAppService, uiServices, () => mainViewModel), "svg:/Assets/angor-icon.svg"),
            new Separator(),
            new Section("Wallet", () => new WalletSectionViewModel(walletAppService, walletFactory, walletBuilder, uiServices, walletRepository), "fa-wallet"),
            new Section("Browse", () => new NavigationViewModel(navigator => new BrowseSectionViewModel(projectService, navigator, uiServices)), "fa-magnifying-glass"),
            new Section("Portfolio", () => new PortfolioSectionViewModel(), "fa-hand-holding-dollar"),
            new Section("Founder", () => new FounderSectionViewModel(projectService), "fa-money-bills"),
            new Separator(),
            new Section("Settings", () => null, "fa-gear"),
            new CommandSection("Angor Hub", ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(Constants.AngorHubUri)), "fa-magnifying-glass") { IsPrimary = false }
        ];

        mainViewModel = new MainViewModel(sections, uiServices);

        return mainViewModel;
    }

    private static WalletAppService WalletApplicationService(AngorWalletDataRepository repository)
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
        var transactionSigner = new NBitcoinTransactionSigner(new MasterkeyProvider(repository), Network.TestNet);
        var bitcoinTransactionService = new BitcoinTransactionService(addressTypeDetector, mempoolUtxoRepository, utxoSelector, transactionPreparer, transactionSigner, mempoolTransactionBroadcaster);
        var walletTransactionService = new MempoolSpaceWalletService(Logger.None, new MempoolAddressScanner(Network.TestNet), mempoolTransactionFetcher);
        var blockchainService = new BlockchainService(mempoolUtxoRepository, bitcoinTransactionService, walletTransactionService, mempoolTransactionBroadcaster);
        return new WalletAppService(repository, blockchainService, new AddressService(addressManager), transactionSigner, repository);
    }

    private static ProjectService RealProjectService()
    {
        var loggerFactory = LoggerConfig.CreateFactory();
        return new ProjectService(DependencyFactory.GetIndexerService(loggerFactory), DependencyFactory.GetRelayService(loggerFactory));
    }
}