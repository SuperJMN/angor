using System.Linq;
using AngorApp.UI.Sections.Browse.ProjectLookup;

namespace AngorApp.UI.Sections.Browse;

public class BrowseSectionViewModelDesign : IBrowseSectionViewModel
{
    public BrowseSectionViewModelDesign()
    {
        Projects = SampleData.GetProjects().Select(IProjectViewModel (project) => new ProjectViewModelDesign(project)).ToList();
    }

    public ICollection<IProjectViewModel> Projects { get; }
    public IProjectLookupViewModel ProjectLookupViewModel { get; }
    public IEnhancedCommand<Result<IEnumerable<IProjectViewModel>>> LoadProjects { get; }
}