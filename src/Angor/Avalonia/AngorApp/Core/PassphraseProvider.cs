using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AngorApp.Sections.Wallet.Unlock;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.Validation.Extensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using Zafiro.Avalonia.Dialogs;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace AngorApp.Core;

internal class WalletUnlocker : IWalletUnlocker
{
    private readonly UIServices services;
    private readonly Dictionary<WalletId, string> passphrases = new();
    private readonly Subject<WalletId> unlockedSubject = new();
    private readonly ConcurrentDictionary<WalletId, SemaphoreSlim> locks = new();

    public WalletUnlocker(UIServices services)
    {
        this.services = services;
    }
    
    public async Task<Maybe<string>> Provide(WalletId id)
    {
        var storedPassphrase = passphrases.TryFind(id);
        if (storedPassphrase.HasValue)
        {
            return storedPassphrase;
        }

        var lockObj = locks.GetOrAdd(id, _ => new SemaphoreSlim(1));
        await lockObj.WaitAsync();
        try
        {
            // Verificar de nuevo por si la contraseña se guardó mientras esperábamos
            storedPassphrase = passphrases.TryFind(id);
            if (storedPassphrase.HasValue)
            {
                return storedPassphrase;
            }

            return await Prompt(id);
        }
        finally
        {
            lockObj.Release();
        }
    }

    private Task<Maybe<string>> Prompt(WalletId id)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var passphraseRequestViewModel = new UnlockRequestViewModel();
            await services.Dialog.Show(passphraseRequestViewModel, "Unlock wallet", passphraseRequestViewModel.IsValid());
            return passphraseRequestViewModel.Password.AsMaybe();
        });
    }
    
    public IObservable<WalletId> WalletUnlocked => unlockedSubject.AsObservable();
    public bool IsUnlocked(WalletId id)
    {
        return passphrases.ContainsKey(id);
    }

    public void ConfirmUnlock(WalletId id, string password)
    {
        passphrases[id] = password;
        unlockedSubject.OnNext(id);
    }
}