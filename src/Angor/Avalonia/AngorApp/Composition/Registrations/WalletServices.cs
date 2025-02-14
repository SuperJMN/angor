using Angor.Wallet.Application;
using Angor.Wallet.Domain;
using AngorApp.Design;
using AngorApp.Sections.Wallet.CreateAndRecover;
using Microsoft.Extensions.DependencyInjection;

namespace AngorApp.Composition.Registrations;

public static class WalletServices
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        return services
            .AddSingleton<Func<BitcoinNetwork>>(() => BitcoinNetwork.Testnet)
            .AddSingleton<IWalletAppService, WalletAppServiceDesign>()
            .AddSingleton<Create>()
            .AddSingleton<Recover>();
    }
}