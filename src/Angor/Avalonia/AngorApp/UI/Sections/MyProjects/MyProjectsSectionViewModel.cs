using System.Reactive.Disposables;
using AngorApp.Model.ProjectsV2;
using AngorApp.Model.ProjectsV2.FundProject;
using AngorApp.Model.ProjectsV2.InvestmentProject;
using DynamicData;
using DynamicData.Aggregation;
using Zafiro.UI.Shell.Utils;

namespace AngorApp.UI.Sections.MyProjects;

[Section("My Projects", icon: "fa-regular fa-file-lines", sortIndex: 4)]
[SectionGroup("FOUNDER")]
public partial class MyProjectsSectionViewModel : ReactiveObject, IMyProjectsSectionViewModel, IDisposable
{
    private readonly CompositeDisposable disposable = new();

    public MyProjectsSectionViewModel(
        UIServices uiServices,
        IWalletPortfolio walletPortfolio,
        ICreateProjectFlow createProjectFlow)
    {
        LoadProjects = walletPortfolio.RefreshFoundedProjects;
        LoadProjects.HandleErrorsWith(uiServices.NotificationService, "Failed to load projects").DisposeWith(disposable);

        Projects = walletPortfolio.FoundedProjects;

        ActiveProjectsCount = walletPortfolio.FoundedProjectChanges.FilterOnObservable(IsActive).Count();

        TotalRaised = walletPortfolio.FoundedProjectChanges.TransformOnObservable(item =>
                                    {
                                        if (item is IInvestmentProject inv)
                                            return inv.Raised;
                                        if (item is IFundProject fnd)
                                            return fnd.Funded;
                                        return Observable.Return<IAmountUI>(new AmountUI(0));
                                    })
                                    .ForAggregation()
                                    .Sum(x => x.Sats)
                                    .Select(sats => new AmountUI(sats));

        Create = ReactiveCommand.CreateFromTask(createProjectFlow.CreateProject)
            .Enhance()
            .DisposeWith(disposable);
        Create.HandleErrorsWith(uiServices.NotificationService, "Cannot create project").DisposeWith(disposable);
    }

    private static IObservable<bool> IsActive(IProject item)
    {
        if (item is IFundProject)
        {
            return Observable.Return(true);
        } else if (item is IInvestmentProject investmentProject)
        {
            return investmentProject.IsFundingOpen;
        }
        
        return Observable.Return(false);
    }

    public IReadOnlyCollection<IProject> Projects { get; }
    public IEnhancedCommand<Result<IEnumerable<IProject>>> LoadProjects { get; }
    public IEnhancedCommand<Result<Maybe<string>>> Create { get; }
    public IObservable<int> ActiveProjectsCount { get; }
    public IObservable<IAmountUI> TotalRaised { get; }

    public void Dispose()
    {
        disposable.Dispose();
    }
}
