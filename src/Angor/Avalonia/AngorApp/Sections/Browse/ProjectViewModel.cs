using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Sections.Browse.Details;
using AngorApp.Services;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.Avalonia.Controls.Navigation;

namespace AngorApp.Sections.Browse;

public class ProjectViewModel(
    IWalletProvider walletProvider,
    IProject project,
    INavigator navigator,
    UIServices uiServices,
    WalletAppService walletAppService,
    IWalletUnlockHandler walletUnlockHandler)
    : ReactiveObject, IProjectViewModel
{
    public IProject Project { get; } = project;
    public ICommand GoToDetails { get; set; } = ReactiveCommand.Create(() => navigator.Go(() => new ProjectDetailsViewModel(walletProvider, walletAppService, project, uiServices, walletUnlockHandler)));
}