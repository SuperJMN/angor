using CSharpFunctionalExtensions;

namespace Angor.UI.Model.Projects;

public interface IProjectService
{
    Task<IList<IProject>> Latest();
    Task<Maybe<IProject>> FindById(string projectId);
}