using System.Reactive.Disposables;
using System.Reactive.Linq;
using Angor.Shared.Models;
using Angor.Shared.Services;
using AngorApp.Model;
using DynamicData;

namespace Angor.Model.Implementation.Projects;

public class ProjectService : IProjectService
{
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