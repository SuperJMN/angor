namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class AsyncLazy<T>
{
    private readonly Lazy<Task<T>> lazy;

    public AsyncLazy(Func<Task<T>> factory)
    {
        lazy = new Lazy<Task<T>>(() => factory());
    }

    public Task<T> Value => lazy.Value;

    public bool IsValueCreated => lazy.IsValueCreated;
}