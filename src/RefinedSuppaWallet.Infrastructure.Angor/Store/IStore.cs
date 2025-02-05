using CSharpFunctionalExtensions;

namespace RefinedSuppaWallet.Infrastructure.Angor.Store;

public interface IStore
{
    Task<Result> Save<T>(string key, T data);
    Task<Result<T>> Load<T>(string key);
}