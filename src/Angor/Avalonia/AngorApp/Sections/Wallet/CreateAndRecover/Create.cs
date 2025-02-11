using System.Threading.Tasks;
using Angor.UI.Model;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.Passphrase.Create;
using AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsConfirmation;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Success;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;
using Zafiro.CSharpFunctionalExtensions;
using EncryptionPasswordViewModel = AngorApp.Sections.Wallet.CreateAndRecover.Steps.EncryptionPassword.EncryptionPasswordViewModel;
using SeedWordsViewModel = AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsGeneration.SeedWordsViewModel;
using SummaryAndCreationViewModel = AngorApp.Sections.Wallet.CreateAndRecover.Steps.SummaryAndCreation.SummaryAndCreationViewModel;
using WelcomeViewModel = AngorApp.Sections.Wallet.CreateAndRecover.Steps.CreateWelcome.WelcomeViewModel;

namespace AngorApp.Sections.Wallet.CreateAndRecover;

public class Create(UIServices uiServices, IWalletBuilder walletBuilder, IWalletAppService walletAppService, IWalletUnlockHandler walletUnlockHandler)
{
    public async Task<Maybe<IWallet>> Start()
    {
        Maybe<IWallet> wallet = Maybe<IWallet>.None;

        var wizard = WizardBuilder
            .StartWith(() => new WelcomeViewModel())
            .Then(prev => new SeedWordsViewModel(uiServices))
            .Then(prev => new SeedWordsConfirmationViewModel(prev.Words!))
            .Then(prev => new PassphraseCreateViewModel(prev.SeedWords))
            .Then(prev => new EncryptionPasswordViewModel(prev.SeedWords, prev.Passphrase!))
            .Then(prev => new SummaryAndCreationViewModel(walletAppService, uiServices, walletUnlockHandler, prev.Passphrase, prev.SeedWords, prev.EncryptionKey!, walletBuilder, r => wallet = r.AsMaybe())
            {
                IsRecovery = false,
            })
            .Then(_ => new SuccessViewModel("Wallet created successfully!", "Done"))
            .Build();

        await uiServices.Dialog.Show(wizard, "Create wallet", closeable => wizard.OptionsForCloseable(closeable));

        return wallet;
    }
}