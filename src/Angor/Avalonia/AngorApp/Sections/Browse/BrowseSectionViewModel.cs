using System.Reactive.Linq;
using Angor.UI.Model;
using AngorApp.Sections.Browse.ProjectLookup;
using AngorApp.Services;
using ReactiveUI.SourceGenerators;
using Zafiro.Avalonia.Controls.Navigation;
using Zafiro.Reactive;

namespace AngorApp.Sections.Browse;

public partial class BrowseSectionViewModel : ReactiveObject, IBrowseSectionViewModel
{
    [Reactive] private string? projectId;

    [ObservableAsProperty] private IList<IProjectViewModel>? projects;

    public BrowseSectionViewModel(IProjectService projectService, INavigator navigator, UIServices uiServices)
    {
        ProjectLookupViewModel = new ProjectLookupViewModel(projectService, navigator, uiServices);
        
        LoadLatestProjects = ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(projectService.Latest)
            .Flatten()
            .Select(IProjectViewModel (project) => new ProjectViewModel(project, navigator, uiServices))
            .ToList());

        OpenHub = ReactiveCommand.CreateFromTask(() => uiServices.LauncherService.LaunchUri(new Uri("https://www.angor.io")));
        projectsHelper = LoadLatestProjects.ToProperty(this, x => x.Projects);
        LoadLatestProjects.Execute().Subscribe();
    }

    public IProjectLookupViewModel ProjectLookupViewModel { get; }

    public ReactiveCommand<Unit, IList<IProjectViewModel>> LoadLatestProjects { get; }

    public ReactiveCommand<Unit, Unit> OpenHub { get; set; }
}