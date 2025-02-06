using System.Threading.Tasks;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Password;
using Avalonia.Threading;
using CSharpFunctionalExtensions;
using ReactiveUI.Validation.Extensions;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Core;

internal class EncryptionKeyProvider : IEncryptionKeyProvider
{
    private readonly UIServices uiServices;

    public EncryptionKeyProvider(UIServices uiServices)
    {
        this.uiServices = uiServices;
    }
    
    public Task<Result<string>> GetEncryptionKey(WalletId walletId)
    {
        return Dispatcher.UIThread.InvokeAsync(() => uiServices.Dialog.ShowAndGetResult(new PasswordViewModel("Please, enter the encryption key"), "Encryption key", x => x.IsValid(), x => x.Password!).ToResult("Cancelled"));
    }
}