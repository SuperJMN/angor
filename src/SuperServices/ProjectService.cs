using System.Reactive.Disposables;
using System.Reactive.Linq;
using Angor.Shared.Models;
using Angor.Shared.Services;
using DynamicData;
using Nostr.Client.Messages.Metadata;

namespace SuperServices;

public class ProjectService
{
    private readonly IIndexerService _indexerService;
    private readonly IRelayService _relayService;

    public ProjectService(IIndexerService indexerService, IRelayService relayService)
    {
        _indexerService = indexerService;
        _relayService = relayService;
    }

    public IObservable<IChangeSet<ProjectData, string>> Connect()
    {
        var tuples = Observable.FromAsync(() => _indexerService
                .GetProjectsAsync(null, 21))
            .SelectMany(projectIndexerDatas => GetProjectInfo(projectIndexerDatas)
                .ToList()
                .SelectMany(projectInfos => GetProjectMetadatas(projectInfos).ToList().Select(metadatas => new
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
                    });
        });

        return observable.ToObservableChangeSet(x => x.IndexerData.ProjectIdentifier);
    }

    private IObservable<ProjectInfo> GetProjectInfo(IList<ProjectIndexerData> list)
    {
        return Observable.Create<ProjectInfo>(observer =>
        {
            _relayService.LookupProjectsInfoByEventIds<ProjectInfo>(
                observer.OnNext,
                observer.OnCompleted,
                list.Select(x => x.NostrEventId).ToArray()
            );

            return Disposable.Empty;
        });
    }

    private IObservable<(string, ProjectMetadata)> GetProjectMetadatas(IList<ProjectInfo> list)
    {
        return Observable.Create<(string, ProjectMetadata)>(observer =>
        {
            _relayService.LookupNostrProfileForNPub((npub, nostrMetadata) => observer.OnNext((npub, nostrMetadata)), () => observer.OnCompleted(), list.Select(x => x.NostrPubKey).ToArray());

            return Disposable.Empty;
        });
    }
}