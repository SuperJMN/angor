using Angor.UI.Model;
using Angor.UI.Model.Implementation.Projects;
using Angor.UI.Model.Projects;
using Angor.UI.Model.Wallet;
using AngorApp.Design;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using AngorApp.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AngorApp.Composition.Registrations;

public static class ModelServices
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        return services
            .AddSingleton<IWalletProvider, WalletProviderDesign>()
            .AddSingleton<IWalletBuilder, WalletBuilderDesign>()
            .AddSingleton<IWalletFactory, WalletFactory>()
            .AddSingleton<IWalletUnlockHandler, WalletUnlockHandlerDesign>()
            .AddSingleton<IProjectService, ProjectService>();
    }
}