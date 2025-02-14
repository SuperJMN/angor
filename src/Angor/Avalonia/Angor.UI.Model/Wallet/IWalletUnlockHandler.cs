using Angor.Wallet.Domain;
using CSharpFunctionalExtensions;

namespace Angor.UI.Model.Wallet;

public interface IWalletUnlockHandler
{
    Task<Maybe<string>> RequestPassword(WalletId id);
    IObservable<WalletId> WalletUnlocked { get; }
    bool IsUnlocked(WalletId id);
    void ConfirmUnlock(WalletId id, string password);
}