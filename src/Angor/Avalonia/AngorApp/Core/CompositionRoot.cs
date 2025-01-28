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
using AngorApp.Services;
using Avalonia.Controls.Notifications;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NBitcoin;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Address;
using RefinedSuppaWalet.Infrastructure.Transactions;
using RefinedSuppaWalet.Infrastructure.Wallet;
using RefinedSuppaWallet.Application.Services.Wallet;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Intrastructure.Mempoo.AddressManager;
using RefinedSuppaWallet.Intrastructure.Mempoo.Repository;
using RefinedSuppaWallet.Intrastructure.Mempoo.TransactionBroadcaster;
using Serilog;
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
            }));
        
        var walletAppService = await WalletApplicationService();
        var walletProvider = new WalletProvider(walletAppService);
        var walletFactory = new WalletFactory(new WalletBuilder(walletAppService), uiServices);

        MainViewModel mainViewModel = null!;

        var projectService = RealProjectService();

        IEnumerable<SectionBase> sections =
        [
            new Section("Home", new HomeSectionViewModel(walletProvider, uiServices, () => mainViewModel), "svg:/Assets/angor-icon.svg"),
            new Separator(),
            new Section("Wallet", new WalletSectionViewModel(walletFactory, walletProvider, uiServices), "fa-wallet"),
            new Section("Browse", new NavigationViewModel(navigator => new BrowseSectionViewModel(walletProvider, projectService, navigator, uiServices)), "fa-magnifying-glass"),
            new Section("Portfolio", new PortfolioSectionViewModel(), "fa-hand-holding-dollar"),
            new Section("Founder", new FounderSectionViewModel(projectService), "fa-money-bills"),
            new Separator(),
            new Section("Settings", null, "fa-gear"),
            new CommandSection("Angor Hub", ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(Constants.AngorHubUri)), "fa-magnifying-glass") { IsPrimary = false }
        ];

        mainViewModel = new MainViewModel(sections, uiServices);

        return mainViewModel;
    }

    private static async Task<WalletAppService> WalletApplicationService()
    {
        var network = Network.TestNet;
        var walletId = WalletId.New();
        var dict = new Dictionary<WalletId, (Network Network, ExtKey ExtKey)>()
        {
            [walletId] = (Network.TestNet, ExtKey.Parse("tprv8ZgxMBicQKsPd3bePirSewfCg7PQ9KaJ1ztgecjDodoit4yt8zns8AMUQhUFVJNLZgaW9AKKnTKHoLNMuqwBPWxucTW1Vh9F59HC2H9Fro3", Network.TestNet))
        };

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
        var walletRepository = new WalletRepository(mempoolSpaceWalletService);
        
        await walletRepository.ImportWallet(walletId, SampleData.WalletDescriptor());
        return new WalletAppService(bitcoinTransactionService, walletRepository);
    }

    private static ProjectService RealProjectService()
    {
        var loggerFactory = LoggerConfig.CreateFactory();
        return new ProjectService(DependencyFactory.GetIndexerService(loggerFactory), DependencyFactory.GetRelayService(loggerFactory));
    }
}