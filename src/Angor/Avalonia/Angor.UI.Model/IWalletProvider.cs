using CSharpFunctionalExtensions;
using RefinedSuppaWallet.Domain;

namespace Angor.UI.Model;

public interface IWalletProvider
{
    Task<Maybe<WalletId>> GetWalletId();
    void SetWallet(WalletId wallet);
}