using CSharpFunctionalExtensions;

namespace SuppaWallet.Gui.Model;

public interface IWalletFactory
{
    public Task<Maybe<IWallet>> Recover();
    public Task<Maybe<IWallet>> Create();
}