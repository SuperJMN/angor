using Angor.Contexts.Funding.Projects.Application.Dtos;
using AngorApp.Sections.Browse;
using AngorApp.Sections.Founder;
using AngorApp.Sections.Founder.Details;
using AngorApp.Sections.Home;
using AngorApp.Sections.Portfolio;
using AngorApp.Sections.Shell;
using AngorApp.Sections.Wallet;
using Microsoft.Extensions.DependencyInjection;

namespace AngorApp.Composition.Registrations;

public static class ViewModels
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        return services
            .AddSingleton<IHomeSectionViewModel, HomeSectionViewModel>()
            .AddSingleton<IWalletSectionViewModel, WalletSectionViewModel>()
            .AddSingleton<IBrowseSectionViewModel, BrowseSectionViewModel>()
            .AddSingleton<IPortfolioSectionViewModel, PortfolioSectionViewModel>()
            .AddSingleton<IFounderSectionViewModel, FounderSectionViewModel>()
            .AddSingleton<Func<ProjectDto, IFounderProjectViewModel>>(provider => dto => ActivatorUtilities.CreateInstance<FounderProjectViewModel>(provider, dto))
            .AddSingleton<Func<ProjectDto, IFounderProjectDetailsViewModel>>(provider => dto => ActivatorUtilities.CreateInstance<FounderProjectDetailsViewModel>(provider, dto))
            .AddSingleton<IMainViewModel, MainViewModel>();
    }
}