using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Angor.Contexts.Funding.Projects.Application.Dtos;
using Angor.Contexts.Funding.Projects.Infrastructure.Interfaces;
using AngorApp.Flows;
using AngorApp.Flows.CreateProyect;
using AngorApp.Sections.Founder.CreateProject.FundingStructure;
using AngorApp.Sections.Founder.CreateProject.Preview;
using AngorApp.Sections.Founder.CreateProject.Profile;
using AngorApp.Sections.Founder.CreateProject.Stages;
using AngorApp.Sections.Shell;
using AngorApp.UI.Controls.Common;
using AngorApp.UI.Services;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Zafiro.Avalonia.Dialogs;
using Zafiro.UI.Commands;

namespace AngorApp.Sections.Founder.CreateProject;

public class CreateProjectViewModel : ReactiveValidationObject, ICreateProjectViewModel, IHaveHeader
{
    private readonly IProjectAppService projectAppService;
    private readonly CompositeDisposable disposable = new();

    public CreateProjectViewModel(
        IWallet wallet,
        CreateProjectFlow.ProjectSeed projectSeed,
        UIServices uiServices,
        IProjectAppService projectAppService,
        IFundingStructureViewModel? fundingStructureViewModel = null,
        IStagesViewModel? stagesViewModel = null,
        IProfileViewModel? profileViewModel = null)
    {
        this.projectAppService = projectAppService;

        var resolvedFundingStructure = fundingStructureViewModel ?? new FundingStructureViewModel();
        TrackForDisposal(resolvedFundingStructure);
        FundingStructureViewModel = resolvedFundingStructure;

        var resolvedStages = stagesViewModel ?? CreateStagesViewModel(resolvedFundingStructure, uiServices);
        TrackForDisposal(resolvedStages);
        StagesViewModel = resolvedStages;

        var resolvedProfile = profileViewModel ?? new ProfileViewModel(projectSeed, uiServices);
        TrackForDisposal(resolvedProfile);
        ProfileViewModel = resolvedProfile;

        StagesViewModel.LastStageDate
            .Select(date => date?.AddDays(60))
            .Subscribe(date => FundingStructureViewModel.ExpiryDate = date)
            .DisposeWith(disposable);

        this.ValidationRule(StagesViewModel.IsValid, b => b, _ => "Stages are not valid").DisposeWith(disposable);
        this.ValidationRule(FundingStructureViewModel.IsValid, b => b, _ => "Funding structures not valid").DisposeWith(disposable);
        this.ValidationRule(ProfileViewModel.IsValid, b => b, _ => "Profile not valid").DisposeWith(disposable);

        Create = ReactiveCommand.CreateFromTask(() =>
        {
            var feerateSelector = new FeerateSelectionViewModel(uiServices);
            var feerate = uiServices.Dialog.ShowAndGetResult(feerateSelector, "Select the feerate", f => f.IsValid, viewModel => viewModel.Feerate!.Value);
            return feerate.ToResult("Choosing a feerate is mandatory")
                .Bind(fr => DoCreateProject(wallet, this.ToDto(), fr))
                .TapError(() => uiServices.NotificationService.Show("An error occurred while creating the project. Please try again.", "Failed to create project"));
        }, IsValid).Enhance();
    }

    private static IStagesViewModel CreateStagesViewModel(IFundingStructureViewModel fundingStructureViewModel, UIServices uiServices)
    {
        return fundingStructureViewModel switch
        {
            FundingStructureViewModel concrete => new StagesViewModel(concrete.WhenAnyValue(x => x.FundingEndDate), uiServices),
            _ => throw new ArgumentException("A custom funding structure view model requires providing a stages view model.", nameof(fundingStructureViewModel))
        };
    }

    private void TrackForDisposal(object instance)
    {
        if (instance is IDisposable disposableInstance)
        {
            disposable.Add(disposableInstance);
        }
    }

    public IEnhancedCommand<Result<string>> Create { get; }

    private async Task<Result<string>> DoCreateProject(IWallet wallet, CreateProjectDto dto, long feeRate)
    {
        var result = await projectAppService.CreateProject(wallet.Id.Value, feeRate, dto);
        if(result.IsSuccess)
            return Result.Success(result.Value.TransactionId); //TODO Jose, need to fix this when the changes are implemented in the UI
        return Result.Failure<string>(result.Error);
    }
    
    public IObservable<bool> IsValid => this.IsValid();
    public IStagesViewModel StagesViewModel { get; }
    public IProfileViewModel ProfileViewModel { get; }
    public IFundingStructureViewModel FundingStructureViewModel { get; }

    public object? Header => new PreviewHeaderViewModel(this);

    protected override void Dispose(bool disposing)
    {
        disposable.Dispose();
        base.Dispose(disposing);
    }
}