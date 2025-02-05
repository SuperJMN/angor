using CSharpFunctionalExtensions;

namespace Angor.UI.Model;

public interface IWalletFactory
{
    public Task<Maybe<IWallet>> Recover();
    public Task<Maybe<IWallet>> Create();
}