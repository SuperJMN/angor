using Angor.Shared.Models;
using RefinedSuppaWallet.Infrastructure.Angor;

namespace Angor.UI.Model.Implementation.Tests;

public class TestingPasswordComponent : IPasswordComponent
{
    public Task<WalletWords> GetWalletAsync()
    {
        return Task.FromResult(new WalletWords()
        {
            Passphrase = "fake",
            Words = "fake"
        });
    }

    public bool HasPassword()
    {
        return true;
    }
}