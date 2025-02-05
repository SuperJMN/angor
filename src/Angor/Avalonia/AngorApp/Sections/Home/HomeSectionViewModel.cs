using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using AngorApp.Core;
using AngorApp.Sections.Shell;
using AngorApp.Services;
using RefinedSuppaWallet.Application;

namespace AngorApp.Sections.Home;

public class HomeSectionViewModel : ReactiveObject, IHomeSectionViewModel
{
    public HomeSectionViewModel(WalletAppService walletAppService, UIServices uiServices, Func<IMainViewModel> getMainViewModel)
    {
        GoToWalletSection = ReactiveCommand.Create(() => getMainViewModel().GoToSection("Wallet"), Observable.FromAsync(walletAppService.GetWallets).Select(x => !x.Any()));
        OpenHub = ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(Constants.AngorHubUri));
        IsWalletSetup = false;
    }

    public bool IsWalletSetup { get; }
    public ICommand GoToWalletSection { get; }
    public ICommand OpenHub { get; }
}