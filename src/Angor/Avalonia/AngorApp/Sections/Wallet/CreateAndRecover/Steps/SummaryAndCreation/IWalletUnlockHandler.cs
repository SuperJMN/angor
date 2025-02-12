using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SuppaWallet.Domain;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;

public interface IWalletUnlockHandler
{
    Task<Maybe<string>> RequestPassword(WalletId id);
    IObservable<WalletId> WalletUnlocked { get; }
    bool IsUnlocked(WalletId id);
    void ConfirmUnlock(WalletId id, string password);
}