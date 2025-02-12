using System.Threading.Tasks;
using AngorApp.Sections.Wallet.CreateAndRecover;
using CSharpFunctionalExtensions;
using SuppaWallet.Gui.Model;

namespace AngorApp.UI.Services;

public class WalletFactory(Create create, Recover recover) : IWalletFactory
{
    public Task<Maybe<IWallet>> Recover()
    {
        return recover.Start();
    }

    public Task<Maybe<IWallet>> Create()
    {
        return create.Start();
    }
}