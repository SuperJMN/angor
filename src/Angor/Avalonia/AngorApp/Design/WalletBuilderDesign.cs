using System.Threading.Tasks;
using AngorApp.Sections.Wallet;
using CSharpFunctionalExtensions;
using SuppaWallet.Domain;
using SuppaWallet.Gui.Model;

namespace AngorApp.Design;

public class WalletBuilderDesign : IWalletBuilder
{
    public async Task<Result<IWallet>> Create(WalletId walletId)
    {
        await Task.Delay(2000);
        return new WalletDesign();
    }
}