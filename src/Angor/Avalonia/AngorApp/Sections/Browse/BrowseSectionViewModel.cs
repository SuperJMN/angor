using AngorApp.Model;
using AngorApp.Services;
using DynamicData;
using Zafiro.Avalonia.Controls.Navigation;

namespace AngorApp.Sections.Browse;

public class BrowseSectionViewModel : ReactiveObject, IBrowseSectionViewModel
{
    public BrowseSectionViewModel(IWalletProvider walletProvider, IProjectService projectService, INavigator navigator, UIServices uiServices)
    {
        projectService.Connect()
            .Transform(project => new ProjectViewModel(walletProvider, project, navigator, uiServices))
            .Bind(out var projects)
            .Subscribe();
        
        Projects = projects;
        OpenHub = ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(new Uri("https://www.angor.io")));
    }

    public ReactiveCommand<Unit, Unit> OpenHub { get; set; }

    public IReadOnlyCollection<ProjectViewModel> Projects { get; }
}