using System.Reactive.Disposables;
using System.Reactive.Linq;
using Angor.Shared.Models;
using Angor.Shared.Services;
using DynamicData;

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
        return Observable.FromAsync(() => _indexerService
                .GetProjectsAsync(null, 21))
            .SelectMany(projectIndexerDatas => GetProjectMetadatas(projectIndexerDatas).ToList().Select(list => new { IndexerDatas = projectIndexerDatas, NostrDatas = list }))
            .Select(arg => arg.IndexerDatas.Join(arg.NostrDatas, x => x.ProjectIdentifier, x => x.ProjectIdentifier, (x, y) => new ProjectData()
            {
                IndexerData = x,
                ProjectInfo = y,
                NostrMetadata = null,
            }))
            .ToObservableChangeSet(x => x.IndexerData.ProjectIdentifier);
    }

    private IObservable<ProjectInfo> GetProjectMetadatas(List<ProjectIndexerData> list)
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
}