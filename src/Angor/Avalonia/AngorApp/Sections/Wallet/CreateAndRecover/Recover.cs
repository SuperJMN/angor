using System.Threading.Tasks;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Create;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Recover;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoverySeedWords;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.RecoveryWelcome;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Success;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.CreateAndRecover
{
    public class Recover
    {
        private readonly UIServices uiServices;
        private readonly IWalletBuilder walletBuilder;
        private readonly IWalletImporter walletImporter;
        private readonly IWalletProvider walletProvider;

        public Recover(UIServices uiServices, IWalletBuilder walletBuilder, IWalletImporter walletImporter, IWalletProvider walletProvider)
        {
            this.uiServices = uiServices;
            this.walletBuilder = walletBuilder;
            this.walletImporter = walletImporter;
            this.walletProvider = walletProvider;
        }

        public async Task<Maybe<IWallet>> Start()
        {
            Maybe<IWallet> wallet = Maybe<IWallet>.None;

            var wizardBuilder = WizardBuilder
                .StartWith(() => new RecoveryWelcomeViewModel())
                .Then(_ => new RecoverySeedWordsViewModel())
                .Then(seedwords => new PassphraseRecoverViewModel(seedwords.SeedWords))
                .Then(passphrase => new EncryptionPasswordViewModel(passphrase.SeedWords, passphrase.Passphrase!))
                .Then(passphrase => new SummaryAndCreationViewModel(walletImporter, walletProvider, passphrase.Passphrase, passphrase.SeedWords, passphrase.Password!, walletBuilder, r => wallet = r.AsMaybe())
                {
                    IsRecovery = true
                })
                .Then(_ => new SuccessViewModel("Wallet recovered!", "Success"))
                .Build();

            await uiServices.Dialog.Show(wizardBuilder, "Recover wallet", closeable => wizardBuilder.OptionsForCloseable(closeable));
            return wallet;
        }
    }
}