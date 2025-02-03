using System.Threading.Tasks;
using AngorApp.Sections.Wallet.Unlock;
using AngorApp.Services;
using CSharpFunctionalExtensions;
using ReactiveUI.Validation.Extensions;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;
using Zafiro.Avalonia.Dialogs;
using Dispatcher = Avalonia.Threading.Dispatcher;

namespace AngorApp.Core;

internal class PassphraseProvider : IPassphraseProvider
{
    private readonly UIServices services;
    private readonly Dictionary<WalletId, string> passphrases = new();

    public PassphraseProvider(UIServices services)
    {
        this.services = services;
    }

    public async Task<Maybe<string>> Provide(WalletId id)
    {
        var promptForPassphrase = await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var passphraseRequestViewModel = new PassphraseRequestViewModel();
            await services.Dialog.Show(passphraseRequestViewModel, "Unlock wallet", passphraseRequestViewModel.IsValid());
            return passphraseRequestViewModel.Passphrase.AsMaybe();
        });
        
        promptForPassphrase.Execute(s => passphrases[id] = s);
        
        return passphrases.TryFind(id).Or(promptForPassphrase);
    }
}