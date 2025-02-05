using System.Windows.Input;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.Operate.Send;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Success;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;
using TransactionPreviewViewModel = AngorApp.UI.Controls.Common.TransactionPreview.TransactionPreviewViewModel;

namespace AngorApp.Sections.Wallet.Operate;

public class WalletViewModel : ReactiveObject, IWalletViewModel
{
    private readonly UIServices uiService;

    public WalletViewModel(IWallet wallet, UIServices uiServices)
    {
        Wallet = wallet;
        uiService = uiServices;

        Wallet.Load.Execute().Subscribe();
    }

    public IWallet Wallet { get; }

    public ICommand Send => ReactiveCommand.CreateFromTask(() =>
    {
        var wizard = WizardBuilder.StartWith(() => new AddressAndAmountViewModel(Wallet))
            .Then(model => new TransactionPreviewViewModel(Wallet, new Destination("Test", model.Amount!.Value, model.Address!), uiService))
            .Then(_ => new SuccessViewModel("Transaction sent!", "Success"))
            .Build();

        return uiService.Dialog.Show(wizard, "Send", closeable => wizard.OptionsForCloseable(closeable));
    });
}