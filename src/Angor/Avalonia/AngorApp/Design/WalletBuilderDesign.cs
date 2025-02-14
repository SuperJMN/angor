using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Domain;
using AngorApp.Sections.Wallet;
using CSharpFunctionalExtensions;

namespace AngorApp.Design;

public class WalletBuilderDesign : IWalletBuilder
{
    public async Task<Result<IWallet>> Create(WalletId walletId)
    {
        await Task.Delay(2000);
        return new WalletDesign();
    }
}