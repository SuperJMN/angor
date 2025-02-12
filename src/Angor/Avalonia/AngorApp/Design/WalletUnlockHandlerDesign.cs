using System.Reactive.Linq;
using System.Threading.Tasks;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using CSharpFunctionalExtensions;
using SuppaWallet.Domain;

namespace AngorApp.Design;

public class WalletUnlockHandlerDesign : IWalletUnlockHandler
{
    public async Task<Maybe<string>> RequestPassword(WalletId id)
    {
        return "";
    }

    public IObservable<WalletId> WalletUnlocked => Observable.Never<WalletId>();

    public bool IsUnlocked(WalletId id)
    {
        return true;
    }

    public void ConfirmUnlock(WalletId id, string password)
    {
    }
}