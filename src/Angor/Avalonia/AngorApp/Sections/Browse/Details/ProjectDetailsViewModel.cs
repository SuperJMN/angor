using System.Threading.Tasks;
using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Implementation;
using AngorApp.Sections.Browse.Details.Invest.Amount;
using AngorApp.Services;
using AngorApp.UI.Controls.Common.Success;
using AngorApp.UI.Controls.Common.TransactionPreview;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Application;
using Zafiro.Avalonia.Controls.Wizards.Builder;
using Zafiro.Avalonia.Dialogs;

namespace AngorApp.Sections.Browse.Details;

public class ProjectDetailsViewModel(IWalletProvider walletProvider, WalletAppService walletAppService, IProject project, UIServices uiServices, IWalletUnlocker walletUnlocker) : ReactiveObject, IProjectDetailsViewModel
{
    public object Icon => project.Icon;
    public object Picture => project.Picture;

    public ICommand Invest { get; } = ReactiveCommand.CreateFromTask(async () =>
    {
        var maybeWallet = await walletProvider.GetWalletId().Map(id => new DynamicWallet(id, walletAppService, walletUnlocker));
        return maybeWallet.Match(wallet => DoInvest(wallet, project, uiServices), () => uiServices.NotificationService.Show("You need to create a Wallet before investing", "No wallet"));
    });

    public IEnumerable<INostrRelay> Relays { get; } =
    [
        new NostrRelayDesign
        {
            Uri = new Uri("wss://relay.angor.io")
        },
        new NostrRelayDesign
        {
            Uri = new Uri("wss://relay2.angor.io")
        }
    ];

    public double TotalDays { get; } = 119;
    public double TotalInvestment { get; } = 1.5d;
    public double CurrentDays { get; } = 11;
    public double CurrentInvestment { get; } = 0.79d;
    public IProject Project => project;

    private static async Task DoInvest(IWallet wallet, IProject project, UIServices uiServices)
    {
        var wizard = WizardBuilder.StartWith(() => new AmountViewModel(wallet, project))
            .Then(viewModel =>
            {
                var destination = new Destination(project.Name, viewModel.Amount!.Value, project.BitcoinAddress);
                return new TransactionPreviewViewModel(wallet, destination, uiServices);
            })
            .Then(_ => new SuccessViewModel("Transaction confirmed!", "Success"))
            .Build();

        await uiServices.Dialog.Show(wizard, @$"Invest in ""{project}""", closeable => wizard.OptionsForCloseable(closeable));
    }
}