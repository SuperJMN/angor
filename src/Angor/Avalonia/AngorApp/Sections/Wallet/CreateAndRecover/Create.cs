using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Application;
using Angor.Wallet.Domain;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.CreateWelcome;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Create;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsConfirmation;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsGeneration;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation;
using AngorApp.UI.Controls.Common.Success;
using AngorApp.UI.Services;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Sections.Wallet.CreateAndRecover;

public class Create(UIServices uiServices, IWalletBuilder walletBuilder, IWalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler, Func<BitcoinNetwork> getNetwork)
{
    public async Task<Maybe<IWallet>> Start()
    {
        var wallet = Maybe<IWallet>.None;

        var wizard = WizardBuilder
            .StartWith(() => new WelcomeViewModel())
            .Then(prev => new SeedWordsViewModel(uiServices))
            .Then(prev => new SeedWordsConfirmationViewModel(prev.Words.Value))
            .Then(prev => new PassphraseCreateViewModel(prev.SeedWords))
            .Then(prev => new EncryptionPasswordViewModel(prev.SeedWords, prev.Passphrase!))
            .Then(prev =>
            {
                var parameters = new WalletSecurityParameters(prev.Passphrase, prev.SeedWords, prev.Password!);
                return new SummaryAndCreationViewModel(walletAppService, walletUnlockHandler, parameters, walletBuilder, uiServices, getNetwork)
                {
                    IsRecovery = false
                };
            })
            .Then(_ => new SuccessViewModel("Wallet created successfully!", "Done"))
            .Build();

        await uiServices.Dialog.Show(wizard, "Create wallet", closeable => wizard.OptionsForCloseable(closeable));

        return wallet;
    }
}