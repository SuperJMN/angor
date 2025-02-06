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
using PasswordViewModel = AngorApp.UI.Controls.Common.Password.PasswordViewModel;

namespace AngorApp.Core;

internal class WalletUnlockHandler : IWalletUnlockHandler
{
    private readonly UIServices services;
    private readonly Dictionary<WalletId, string> passphrases = new();
    private readonly Subject<WalletId> unlockedSubject = new();
    private readonly ConcurrentDictionary<WalletId, SemaphoreSlim> locks = new();

    public WalletUnlockHandler(UIServices services)
    {
        this.services = services;
    }
    
    public async Task<Maybe<string>> RequestPassword(WalletId id)
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
        return Dispatcher.UIThread.InvokeAsync(() => services.Dialog.ShowAndGetResult(new UnlockRequestViewModel(), "Unlock wallet", model =>  model.IsValid(), x => x.Password!));
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