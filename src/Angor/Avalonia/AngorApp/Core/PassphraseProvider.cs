using System.Reactive.Linq;
using System.Reactive.Subjects;
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

    public WalletUnlocker(UIServices services)
    {
        this.services = services;
    }

    public async Task<Maybe<string>> Provide(WalletId id)
    {
        return await passphrases.TryFind(id).Or(() => Prompt(id));
    }

    private Task<Maybe<string>> Prompt(WalletId id)
    {
        var promptForPassphrase = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var passphraseRequestViewModel = new UnlockRequestViewModel();
            await services.Dialog.Show(passphraseRequestViewModel, "Unlock wallet", passphraseRequestViewModel.IsValid());
            return passphraseRequestViewModel.Password.AsMaybe();
        });
        
        promptForPassphrase.Execute(s =>
        {
            passphrases[id] = s;
            unlockedSubject.OnNext(id);
        });

        return promptForPassphrase;
    }

    public IObservable<WalletId> WalletUnlocked => unlockedSubject.AsObservable();
    public bool IsUnlocked(WalletId id)
    {
        return passphrases.ContainsKey(id);
    }
}