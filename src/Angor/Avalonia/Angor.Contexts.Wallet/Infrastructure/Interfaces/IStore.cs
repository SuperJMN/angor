using CSharpFunctionalExtensions;

namespace Angor.Contexts.Wallet.Infrastructure.Interfaces;

public interface IStore
{
    Task<Result> Save<T>(string key, T data);
    Task<Result<T>> Load<T>(string key);
}