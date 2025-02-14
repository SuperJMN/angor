using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Projects;

namespace AngorApp.Sections.Browse.ProjectLookup;

public class ProjectViewModelDesign(IProject project) : IProjectViewModel
{
    public IProject Project { get; } = project;
    public ICommand GoToDetails { get; set; }
}