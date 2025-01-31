namespace RefinedSuppaWallet.Infrastructure.Angor.Store;

public interface IStore
{
    Task Save<T>(string key, T data);
    Task<T?> Load<T>(string key);
}