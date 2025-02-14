using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Projects;

namespace AngorApp.Sections.Browse;

public interface IProjectViewModel
{
    IProject Project { get; }
    public ICommand GoToDetails { get; set; }
}