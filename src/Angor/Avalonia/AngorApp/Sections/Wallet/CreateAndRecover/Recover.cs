using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Application;
using Angor.Wallet.Domain;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Recover;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoverySeedWords;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoveryWelcome;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using AngorApp.UI.Controls.Common.Success;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Sections.Wallet.CreateAndRecover;

public class Recover(UIServices uiServices, IWalletBuilder walletBuilder, IWalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler, Func<BitcoinNetwork> getNetwork)
{
    public async Task<Maybe<IWallet>> Start()
    {
        Maybe<IWallet> wallet = Maybe<IWallet>.None;

        var wizardBuilder = WizardBuilder
            .StartWith(() => new RecoveryWelcomeViewModel())
            .Then(_ => new RecoverySeedWordsViewModel())
            .Then(seedwords => new PassphraseRecoverViewModel(seedwords.SeedWords))
            .Then(passphrase => new EncryptionPasswordViewModel(passphrase.SeedWords, passphrase.Passphrase!))
            .Then(prev =>
            {
                var parameters = new WalletSecurityParameters(prev.Passphrase, prev.SeedWords, prev.Password!);
                return new SummaryAndCreationViewModel(walletAppService, walletUnlockHandler, parameters, walletBuilder, uiServices, getNetwork)
                {
                    IsRecovery = false
                };
            })
            .Then(_ => new SuccessViewModel("Wallet recovered!", "Success"))
            .Build();

        var result = await uiServices.Dialog.Show(wizardBuilder, "Recover wallet",
            closeable => wizardBuilder.OptionsForCloseable(closeable));
        if (result)
        {
            return new WalletDesign();
        }

        return Maybe<IWallet>.None;
    }
}