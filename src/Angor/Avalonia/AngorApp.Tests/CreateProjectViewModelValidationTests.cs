using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Angor.Contexts.Funding.Projects.Application.Dtos;
using Angor.Contexts.Funding.Projects.Domain;
using Angor.Contexts.Funding.Projects.Infrastructure.Interfaces;
using Angor.Contexts.Funding.Shared;
using Angor.Contexts.Wallet.Domain;
using Angor.UI.Model;
using AngorApp.Flows.CreateProyect;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Founder.CreateProject;
using AngorApp.Sections.Founder.CreateProject.FundingStructure;
using AngorApp.Sections.Founder.CreateProject.Profile;
using AngorApp.Sections.Founder.CreateProject.Stages;
using AngorApp.Sections.Founder.CreateProject.Stages.Creator;
using AngorApp.Sections.Shell;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using FluentAssertions;
using ReactiveUI;
using Zafiro.Settings;
using Zafiro.UI.Commands;
using WalletAmount = Angor.Contexts.Wallet.Domain.Amount;
using FundingAmount = Angor.Contexts.Funding.Projects.Domain.Amount;

namespace AngorApp.Tests;

public class CreateProjectViewModelValidationTests
{
    [Fact]
    public void BecomesValidWhenAllSectionsAreValid()
    {
        var fundingStructure = new StubFundingStructureViewModel();
        var stages = new StubStagesViewModel();
        var profile = new StubProfileViewModel();
        var uiServices = new UIServices(new TestDialog(), new TestNotificationService(), new StubValidations(), new StubSettings());
        var projectAppService = new StubProjectAppService();
        var wallet = new StubWallet();

        using var sut = new CreateProjectViewModel(wallet, new CreateProjectFlow.ProjectSeed("pub"), uiServices, projectAppService, fundingStructure, stages, profile);

        bool? latest = null;
        using var subscription = sut.IsValid.Subscribe(value => latest = value);

        latest.Should().BeFalse();

        fundingStructure.SetValidity(true);
        stages.SetValidity(true);
        profile.SetValidity(true);

        sut.IsValid.Where(value => value).Take(1).Wait();

        latest.Should().BeTrue();
    }

    [Fact]
    public void BecomesInvalidWhenASectionTurnsInvalid()
    {
        var fundingStructure = new StubFundingStructureViewModel();
        var stages = new StubStagesViewModel();
        var profile = new StubProfileViewModel();
        var uiServices = new UIServices(new TestDialog(), new TestNotificationService(), new StubValidations(), new StubSettings());
        var projectAppService = new StubProjectAppService();
        var wallet = new StubWallet();

        using var sut = new CreateProjectViewModel(wallet, new CreateProjectFlow.ProjectSeed("pub"), uiServices, projectAppService, fundingStructure, stages, profile);

        bool? latest = null;
        using var subscription = sut.IsValid.Subscribe(value => latest = value);

        fundingStructure.SetValidity(true);
        stages.SetValidity(true);
        profile.SetValidity(true);
        sut.IsValid.Where(value => value).Take(1).Wait();
        latest.Should().BeTrue();

        profile.SetValidity(false);

        sut.IsValid.Where(value => !value).Take(1).Wait();
        latest.Should().BeFalse();
    }

    private sealed class StubFundingStructureViewModel : IFundingStructureViewModel
    {
        private readonly BehaviorSubject<bool> isValid = new(false);

        public IObservable<bool> IsValid => isValid;

        public void SetValidity(bool value) => isValid.OnNext(value);

        public long? Sats { get; set; }
        public DateTime FundingStartDate { get; } = DateTime.UtcNow;
        public int? PenaltyDays { get; set; }
        public DateTime? FundingEndDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public IAmountUI TargetAmount { get; } = new StubAmountUI();
        public ICollection<string> Errors { get; } = new List<string>();
    }

    private sealed class StubStagesViewModel : IStagesViewModel
    {
        private readonly BehaviorSubject<bool> isValid = new(false);
        private readonly BehaviorSubject<DateTime?> lastStageDate = new(null);

        public void SetValidity(bool value) => isValid.OnNext(value);

        public IEnhancedCommand AddStage { get; } = ReactiveCommand.Create(() => { }).Enhance();
        public ICollection<ICreateProjectStage> Stages { get; } = new List<ICreateProjectStage>();
        public IObservable<bool> IsValid => isValid;
        public IObservable<DateTime?> LastStageDate => lastStageDate;
        public IStagesCreatorViewModel StagesCreator { get; } = new StagesCreatorViewModel();
        public IEnhancedCommand CreateStages { get; } = ReactiveCommand.Create(() => { }).Enhance();
        public ICollection<string> Errors { get; } = new List<string>();
    }

    private sealed class StubProfileViewModel : IProfileViewModel
    {
        private readonly BehaviorSubject<bool> isValid = new(false);

        public IObservable<bool> IsValid => isValid;

        public void SetValidity(bool value) => isValid.OnNext(value);

        public string? ProjectName { get; set; }
        public string? WebsiteUri { get; set; }
        public string? Description { get; set; }
        public string? AvatarUri { get; set; }
        public string? BannerUri { get; set; }
        public string? Nip05Username { get; set; }
        public string? LightningAddress { get; set; }
        public ICollection<string> Errors { get; } = new List<string>();
    }

    private sealed class StubAmountUI : IAmountUI
    {
        public long Sats => 0;
        public string Symbol => "sat";
    }

    private sealed class StubWallet : IWallet
    {
        public ReadOnlyObservableCollection<IBroadcastedTransaction> History { get; } = new(new ObservableCollection<IBroadcastedTransaction>());
        public IAmountUI Balance { get; } = new StubAmountUI();
        public Result IsAddressValid(string address) => Result.Success();
        public WalletId Id { get; } = WalletId.New();
        public IEnhancedCommand Send { get; } = ReactiveCommand.Create(() => { }).Enhance();
        public IEnhancedCommand<Result<string>> GetReceiveAddress { get; } = ReactiveCommand.Create(() => Result.Success(string.Empty)).Enhance();
        public Task<Result<string>> GenerateReceiveAddress() => Task.FromResult(Result.Success(string.Empty));
        public IEnhancedCommand GetTestCoins { get; } = ReactiveCommand.Create(() => { }).Enhance();
    }

    private sealed class StubProjectAppService : IProjectAppService
    {
        public Task<Result<IEnumerable<ProjectDto>>> Latest() => Task.FromResult(Result.Success<IEnumerable<ProjectDto>>(Array.Empty<ProjectDto>()));
        public Task<Maybe<ProjectDto>> FindById(ProjectId projectId) => Task.FromResult(Maybe<ProjectDto>.None);
        public Task<Result<ProjectDto>> Get(ProjectId projectId) => Task.FromResult(Result.Failure<ProjectDto>("not used"));
        public Task<Result<IEnumerable<ProjectDto>>> GetFounderProjects(Guid walletId) => Task.FromResult(Result.Success<IEnumerable<ProjectDto>>(Array.Empty<ProjectDto>()));
        public Task<Result<TransactionDraft>> CreateProject(Guid walletId, long selectedFee, CreateProjectDto project) => Task.FromResult(Result.Success(new TransactionDraft
        {
            SignedTxHex = string.Empty,
            TransactionId = Guid.NewGuid().ToString(),
            TransactionFee = new FundingAmount(0)
        }));
        public Task<Result<ProjectStatisticsDto>> GetProjectStatistics(ProjectId projectId) => Task.FromResult(Result.Failure<ProjectStatisticsDto>("not used"));
    }

    private sealed class StubValidations : IValidations
    {
        public Task<Result> CheckNip05Username(string username, string nostrPubKey) => Task.FromResult(Result.Success());

        public Task<Result> CheckLightningAddress(string lightningAddress) => Task.FromResult(Result.Success());
    }

    private sealed class StubSettings : ISettings<UIPreferences>
    {
        private UIPreferences current = UIPreferences.CreateDefault();
        private readonly BehaviorSubject<UIPreferences> subject;

        public StubSettings()
        {
            subject = new BehaviorSubject<UIPreferences>(current);
        }

        public IObservable<UIPreferences> Changes => subject;

        public Result<UIPreferences> Get() => Result.Success(current);

        public Result Reload()
        {
            subject.OnNext(current);
            return Result.Success();
        }

        public Result Update(Func<UIPreferences, UIPreferences> update)
        {
            current = update(current);
            subject.OnNext(current);
            return Result.Success();
        }
    }
}
