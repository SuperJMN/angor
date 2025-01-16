using DynamicData;

namespace AngorApp.Model;

public interface IProjectService
{
    IObservable<IChangeSet<IProject, string>> Connect();
}