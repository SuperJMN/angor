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

    public PassphraseProvider(UIServices services)
    {
        this.services = services;
    }

    public Task<Maybe<string>> Provide(WalletId id)
    {
        return Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var passphraseRequestViewModel = new PassphraseRequestViewModel();
            await services.Dialog.Show(passphraseRequestViewModel, "Unlock wallet", passphraseRequestViewModel.IsValid());
            return passphraseRequestViewModel.Passphrase.AsMaybe();
        });
    }
}