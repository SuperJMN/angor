using System.Collections.ObjectModel;
using AngorApp.Model.Funded.Shared.Model;
using AngorApp.Model.ProjectsV2;
using DynamicData;

namespace AngorApp.Model.Shared.Services;

public interface IWalletPortfolio : IDisposable
{
    ReadOnlyObservableCollection<IFunded> Investments { get; }
    IObservable<IChangeSet<IFunded, string>> InvestmentChanges { get; }
    IEnhancedCommand<Result<IEnumerable<IFunded>>> RefreshInvestments { get; }

    ReadOnlyObservableCollection<IProject> FoundedProjects { get; }
    IObservable<IChangeSet<IProject, string>> FoundedProjectChanges { get; }
    IEnhancedCommand<Result<IEnumerable<IProject>>> RefreshFoundedProjects { get; }
}
