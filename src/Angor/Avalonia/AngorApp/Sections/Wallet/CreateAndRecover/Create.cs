using System.Threading.Tasks;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.CreateWelcome;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Create;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsConfirmation;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsGeneration;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using AngorApp.UI.Controls.Common.Success;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using SuppaWallet.Application.Interfaces;
using SuppaWallet.Gui.Model;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.CreateAndRecover;

public class Create
{
    private readonly UIServices uiServices;
    private readonly IWalletBuilder walletBuilder;
    private readonly IWalletImporter walletImporter;
    private readonly IWalletProvider walletProvider;
    private readonly IWalletUnlockHandler walletUnlockHandler;

    public Create(UIServices uiServices, IWalletBuilder walletBuilder, IWalletImporter walletImporter, IWalletProvider walletProvider, IWalletUnlockHandler walletUnlockHandler)
    {
        this.uiServices = uiServices;
        this.walletBuilder = walletBuilder;
        this.walletImporter = walletImporter;
        this.walletProvider = walletProvider;
        this.walletUnlockHandler = walletUnlockHandler;
    }

    public async Task<Maybe<IWallet>> Start()
    {
        Maybe<IWallet> wallet = Maybe<IWallet>.None;

        var wizard = WizardBuilder
            .StartWith(() => new WelcomeViewModel())
            .Then(prev => new SeedWordsViewModel(uiServices))
            .Then(prev => new SeedWordsConfirmationViewModel(prev.Words.Value))
            .Then(prev => new PassphraseCreateViewModel(prev.SeedWords))
            .Then(prev => new EncryptionPasswordViewModel(prev.SeedWords, prev.Passphrase!))
            .Then(prev => new SummaryAndCreationViewModel(walletImporter, walletUnlockHandler, prev.Passphrase, prev.SeedWords, prev.Password!, walletBuilder, r => wallet = r.AsMaybe())
            {
                IsRecovery = false
            })
            .Then(_ => new SuccessViewModel("Wallet created successfully!", "Done"))
            .Build();

        await uiServices.Dialog.Show(wizard, "Create wallet", closeable => wizard.OptionsForCloseable(closeable));

        return wallet;
    }
}