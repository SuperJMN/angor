using System.Threading.Tasks;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Password;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using ReactiveUI.Validation.Extensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Core;

internal class PassphraseProvider : IPassphraseProvider
{
    private readonly UIServices uiServices;

    public PassphraseProvider(UIServices uiServices)
    {
        this.uiServices = uiServices;
    }

    public Task<Result<string>> RequestPassphrase(WalletId walletId)
    {
        return Dispatcher.UIThread.InvokeAsync(() => uiServices.Dialog.ShowAndGetResult(new PasswordViewModel("Please, enter the passphrase"), "Passphrase", x => x.IsValid(), x => x.Password!).ToResult("Cancelled"));
    }
}