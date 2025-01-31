using CSharpFunctionalExtensions;

namespace Angor.UI.Model;

public interface IWalletProvider
{
    Task<Maybe<IWallet>> GetWallet();
    void SetWallet(IWallet wallet);
}