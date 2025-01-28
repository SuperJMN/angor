using Angor.Shared.Models;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public interface IPasswordComponent
{
    Task<WalletWords> GetWalletAsync();
    bool HasPassword();
}