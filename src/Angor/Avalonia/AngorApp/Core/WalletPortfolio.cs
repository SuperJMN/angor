using System.Collections.ObjectModel;
using Angor.Sdk.Funding.Investor;
using Angor.Sdk.Funding.Investor.Operations;
using Angor.Sdk.Funding.Projects;
using Angor.Sdk.Funding.Projects.Operations;
using Angor.Sdk.Funding.Shared;
using AngorApp.Core.Factories;
using AngorApp.Model.Funded.Fund.Model;
using AngorApp.Model.Funded.Investment.Model;
using AngorApp.Model.Funded.Shared.Model;
using AngorApp.Model.ProjectsV2;
using AngorApp.Model.ProjectsV2.FundProject;
using AngorApp.Model.ProjectsV2.InvestmentProject;
using DynamicData;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Core;

public sealed class WalletPortfolio : IWalletPortfolio
{
    private readonly IInvestmentAppService investmentAppService;
    private readonly IProjectAppService projectAppService;
    private readonly IWalletContext walletContext;
    private readonly IProjectFactory projectFactory;
    private readonly INotificationService notificationService;
    private readonly ITransactionDraftPreviewer draftPreviewer;

    private readonly RefreshableCollection<IFunded, string> investmentsCollection;
    private readonly RefreshableCollection<IProject, string> foundedProjectsCollection;

    public WalletPortfolio(
        IInvestmentAppService investmentAppService,
        IProjectAppService projectAppService,
        IWalletContext walletContext,
        IProjectFactory projectFactory,
        INotificationService notificationService,
        ITransactionDraftPreviewer draftPreviewer)
    {
        this.investmentAppService = investmentAppService;
        this.projectAppService = projectAppService;
        this.walletContext = walletContext;
        this.projectFactory = projectFactory;
        this.notificationService = notificationService;
        this.draftPreviewer = draftPreviewer;

        investmentsCollection = RefreshableCollection.Create(
            GetInvestments,
            funded => funded.Project.Id.Value,
            funded => funded.InvestorData.InvestedOn == DateTimeOffset.MinValue
                ? long.MaxValue
                : -funded.InvestorData.InvestedOn.UtcTicks);

        Investments = investmentsCollection.Items;
        InvestmentChanges = investmentsCollection.Changes;
        RefreshInvestments = investmentsCollection.Refresh;

        foundedProjectsCollection = RefreshableCollection.Create(
            GetFoundedProjects,
            project => project.Id.Value);

        FoundedProjects = foundedProjectsCollection.Items;
        FoundedProjectChanges = foundedProjectsCollection.Changes;
        RefreshFoundedProjects = foundedProjectsCollection.Refresh;
    }

    public ReadOnlyObservableCollection<IFunded> Investments { get; }
    public IObservable<IChangeSet<IFunded, string>> InvestmentChanges { get; }
    public IEnhancedCommand<Result<IEnumerable<IFunded>>> RefreshInvestments { get; }

    public ReadOnlyObservableCollection<IProject> FoundedProjects { get; }
    public IObservable<IChangeSet<IProject, string>> FoundedProjectChanges { get; }
    public IEnhancedCommand<Result<IEnumerable<IProject>>> RefreshFoundedProjects { get; }

    private async Task<Result<IEnumerable<IFunded>>> GetInvestments()
    {
        return await walletContext
            .Require()
            .Bind(wallet => investmentAppService.GetInvestments(
                new GetInvestments.GetInvestmentsRequest(wallet.Id)))
            .Map(response => response.Projects)
            .MapSequentially(CreateFunded);
    }

    private Task<Result<IFunded>> CreateFunded(InvestedProjectDto dto)
    {
        return projectAppService
            .Get(new GetProject.GetProjectRequest(new ProjectId(dto.Id)))
            .Map(IFunded (response) =>
            {
                var project = projectFactory.Create(response.Project);
                return project switch
                {
                    IInvestmentProject investmentProject => new InvestmentFunded(
                        investmentProject,
                        new InvestmentInvestorData(dto, investmentAppService, walletContext),
                        notificationService, draftPreviewer, investmentAppService, walletContext),
                    IFundProject fundProject => new FundFunded(
                        fundProject,
                        new FundInvestorData(dto, investmentAppService, walletContext),
                        notificationService, draftPreviewer, investmentAppService, walletContext),
                    _ => throw new ArgumentOutOfRangeException(nameof(project))
                };
            });
    }

    private async Task<Result<IEnumerable<IProject>>> GetFoundedProjects()
    {
        return await walletContext
            .Require()
            .Bind(wallet => projectAppService.GetFounderProjects(wallet.Id))
            .Map(response => response.Projects.Select(projectFactory.Create));
    }

    public void Dispose()
    {
        investmentsCollection.Dispose();
        foundedProjectsCollection.Dispose();
    }
}
