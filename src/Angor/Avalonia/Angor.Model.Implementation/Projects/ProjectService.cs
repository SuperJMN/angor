using System.Reactive.Disposables;
using System.Reactive.Linq;
using Angor.Shared.Models;
using Angor.Shared.Services;
using AngorApp.Model;
using DynamicData;

namespace Angor.Model.Implementation.Projects;

public static class IndexerServiceMixin
{
    record PageState(int Offset, IEnumerable<ProjectIndexerData> Items);
        
    public static IObservable<ProjectIndexerData> GetAllProjectsWithExpand(
        this IIndexerService projectService,
        int pageSize = 20)
    {
        // Clase para llevar estado (offset + lista de items)
            

        return Observable
            .Defer(() =>
                // Primera página, offset = 0
                Observable.FromAsync(() => projectService.GetProjectsAsync(0, pageSize))
                    .Select(items => new PageState(0, items))
            )
            .Expand(state =>
            {
                // Expand definirá cómo obtener la "siguiente" página a partir de la "actual"
                if (state.Items == null || !state.Items.Any())
                {
                    // Si no hay más datos, emitimos secuencia vacía (termina la expansión)
                    return Observable.Empty<PageState>();
                }

                // Siguiente offset
                var nextOffset = state.Offset + pageSize;
                return Observable
                    .FromAsync(() => projectService.GetProjectsAsync(nextOffset, pageSize))
                    .Select(items => new PageState(nextOffset, items));
            })
            // Nos quedamos sólo con las páginas que tengan datos
            .TakeWhile(state => state.Items != null && state.Items.Any())
            // De cada 'PageState', emitimos sus items
            .SelectMany(state => state.Items);
    }
}


public class ProjectService : IProjectService
{
    private const int MaxProjectCount = 21;
    private readonly IIndexerService indexerService;
    private readonly IRelayService relayService;

    public ProjectService(IIndexerService indexerService, IRelayService relayService)
    {
        this.indexerService = indexerService;
        this.relayService = relayService;
    }

    public IObservable<IChangeSet<IProject, string>> Connect()
    {
        var tuples = indexerService.GetAllProjectsWithExpand()
            .Buffer(20)
            .SelectMany(projectIndexerDatas => ProjectInfos(projectIndexerDatas)
                .ToList()
                .SelectMany(projectInfos => ProjectMetadatas(projectInfos).ToList().Select(metadatas => new
                {
                    metadatas, projectInfos, projectIndexerDatas
                })));

        var observable = tuples.Select(x =>
        {
            var infoAndMetadata = x.projectInfos.Join(x.metadatas, projectInfo => projectInfo.NostrPubKey, tuple => tuple.Item1, (info, tuple) => (info: info, Metadata: tuple.Item2));
            return x.projectIndexerDatas
                .Join(
                    infoAndMetadata,
                    projectIndexerData => projectIndexerData.ProjectIdentifier,
                    tuple => tuple.info.ProjectIdentifier,
                    (projectIndexerData, tuple) => new ProjectData
                    {
                        ProjectInfo = tuple.info,
                        NostrMetadata = tuple.Metadata,
                        IndexerData = projectIndexerData,
                    }).Select(data => data.ToProject());
        });

        return observable.ToObservableChangeSet(x => x.Id);
    }

    private IObservable<ProjectInfo> ProjectInfos(IEnumerable<ProjectIndexerData> projectIndexerDatas)
    {
        return Observable.Create<ProjectInfo>(observer =>
        {
            relayService.LookupProjectsInfoByEventIds<ProjectInfo>(
                observer.OnNext,
                observer.OnCompleted,
                projectIndexerDatas.Select(x => x.NostrEventId).ToArray()
            );

            return Disposable.Empty;
        });
    }

    private IObservable<(string, ProjectMetadata)> ProjectMetadatas(IEnumerable<ProjectInfo> projectInfos)
    {
        return Observable.Create<(string, ProjectMetadata)>(observer =>
        {
            relayService.LookupNostrProfileForNPub((npub, nostrMetadata) => observer.OnNext((npub, nostrMetadata)), observer.OnCompleted, projectInfos.Select(x => x.NostrPubKey).ToArray());

            return Disposable.Empty;
        });
    }
}