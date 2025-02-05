using System.Threading.Tasks;
using Angor.UI.Model;
using AngorApp.Sections.Wallet;
using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace AngorApp.Design;

public class WalletBuilderDesign : IWalletBuilder
{
    public async Task<Result<IWallet>> Create(WalletId walletId)
    {
        await Task.Delay(2000);
        return new WalletDesign();
    }
}