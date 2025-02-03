using System.Reactive.Linq;
using Angor.UI.Model;
using AngorApp.Sections.Browse.ProjectLookup;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.SourceGenerators;
using RefinedSuppaWallet.Application;
using Zafiro.Avalonia.Controls.Navigation;
using Zafiro.CSharpFunctionalExtensions;
using Zafiro.Reactive;

namespace AngorApp.Sections.Browse;

public partial class BrowseSectionViewModel : ReactiveObject, IBrowseSectionViewModel
{
    [Reactive] private string? projectId;

    [ObservableAsProperty] private IList<IProjectViewModel>? projects;

    public BrowseSectionViewModel(IWalletProvider walletProvider, IProjectService projectService, INavigator navigator, UIServices uiServices, WalletAppService walletAppService, IPassphraseProvider passphraseProvider)
    {
        ProjectLookupViewModel = new ProjectLookupViewModel(projectService, walletProvider, navigator, uiServices, walletAppService, passphraseProvider);
        
        LoadLatestProjects = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(projectService.Latest)
            .Flatten()
            .Select(IProjectViewModel (project) => new ProjectViewModel(walletProvider, project, navigator, uiServices, walletAppService, passphraseProvider))
            .ToList());

        OpenHub = ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(new Uri("https://www.angor.io")));
        projectsHelper = LoadLatestProjects.ToProperty(this, x => x.Projects);
        LoadLatestProjects.Execute().Subscribe();
    }

    public IProjectLookupViewModel ProjectLookupViewModel { get; }

    public ReactiveCommand<Unit, IList<IProjectViewModel>> LoadLatestProjects { get; }

    public ReactiveCommand<Unit, Unit> OpenHub { get; set; }
}