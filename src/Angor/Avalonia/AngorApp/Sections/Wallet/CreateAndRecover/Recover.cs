using System.Threading.Tasks;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Core;
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
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;
using Zafiro.CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.CreateAndRecover
{
    public class Recover
    {
        private readonly UIServices uiServices;
        private readonly IWalletBuilder walletBuilder;
        private readonly IWalletAppService walletAppService;
        private readonly IWalletUnlockHandler walletUnlockHandler;

        public Recover(UIServices uiServices, IWalletBuilder walletBuilder, IWalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler)
        {
            this.uiServices = uiServices;
            this.walletBuilder = walletBuilder;
            this.walletAppService = walletAppService;
            this.walletUnlockHandler = walletUnlockHandler;
        }

        public async Task<Maybe<IWallet>> Start()
        {
            Maybe<IWallet> wallet = Maybe<IWallet>.None;

            var wizardBuilder = WizardBuilder
                .StartWith(() => new RecoveryWelcomeViewModel())
                .Then(_ => new RecoverySeedWordsViewModel())
                .Then(seedwords => new PassphraseRecoverViewModel(seedwords.SeedWords))
                .Then(passphrase => new EncryptionPasswordViewModel(passphrase.SeedWords, passphrase.Passphrase!))
                .Then(data => new SummaryAndCreationViewModel(
                    walletAppService: walletAppService, 
                    uiServices: uiServices, unlockHandler: walletUnlockHandler, 
                    passphrase: data.Passphrase, 
                    seedwords: data.SeedWords, 
                    encryptionKey: data.EncryptionKey!, 
                    walletBuilder: walletBuilder, r => wallet = r.AsMaybe())
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