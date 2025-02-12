using System.Threading.Tasks;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Recover;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoverySeedWords;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoveryWelcome;
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

public class Recover
{
    private readonly UIServices uiServices;
    private readonly IWalletBuilder walletBuilder;
    private IWalletImporter walletImporter;
    private IWalletUnlockHandler walletUnlockHandler;

    public Recover(UIServices uiServices, IWalletBuilder walletBuilder, IWalletImporter walletImporter, IWalletUnlockHandler walletUnlockHandler)
    {
        this.uiServices = uiServices;
        this.walletBuilder = walletBuilder;
        this.walletImporter = walletImporter;
        this.walletUnlockHandler = walletUnlockHandler;
    }

    public async Task<Maybe<Result<IWallet>>> Start()
    {
        Maybe<IWallet> wallet = Maybe<IWallet>.None;
        
        var wizardBuilder = WizardBuilder
            .StartWith(() => new RecoveryWelcomeViewModel())
            .Then(_ => new RecoverySeedWordsViewModel())
            .Then(seedwords => new PassphraseRecoverViewModel(seedwords.SeedWords))
            .Then(passphrase => new EncryptionPasswordViewModel(passphrase.SeedWords, passphrase.Passphrase!))
            .Then(encryption => new SummaryAndCreationViewModel(walletImporter, walletUnlockHandler, encryption.Passphrase, encryption.SeedWords, encryption.Password!, walletBuilder, x =>  wallet = x.AsMaybe())
            {
                IsRecovery = true
            })
            .Then(_ => new SuccessViewModel("Wallet recovered!", "Success"))
            .Build();

        var result = await uiServices.Dialog.Show(wizardBuilder, "Recover wallet",
            closeable => wizardBuilder.OptionsForCloseable(closeable));
        if (result)
        {
            return Result.Success<IWallet>(new WalletDesign());
        }

        return Maybe<Result<IWallet>>.None;
    }
}