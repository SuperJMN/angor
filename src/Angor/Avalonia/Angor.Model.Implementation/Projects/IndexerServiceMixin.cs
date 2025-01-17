using System.Reactive.Linq;
using Angor.Shared.Services;

namespace Angor.Model.Implementation.Projects;

public static class IndexerServiceMixin
{
    record PageState(int Offset, IEnumerable<ProjectIndexerData> Items);
        
    public static IObservable<ProjectIndexerData> GetAllProjectsWithExpand(
        this IIndexerService projectService,
        int pageSize = 20)
    {
        return Observable
            .Defer(() =>
                Observable.FromAsync(() => projectService.GetProjectsAsync(0, pageSize))
                    .Select(items => new PageState(0, items))
            )
            .Expand(state =>
            {
                if (!state.Items.Any())
                {
                    return Observable.Empty<PageState>();
                }

                var nextOffset = state.Offset + pageSize;
                return Observable
                    .FromAsync(() => projectService.GetProjectsAsync(nextOffset, pageSize))
                    .Select(items => new PageState(nextOffset, items));
            })
            .TakeWhile(state => state.Items.Any())
            .SelectMany(state => state.Items);
    }
}