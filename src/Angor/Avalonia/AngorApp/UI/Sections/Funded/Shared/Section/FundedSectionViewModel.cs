using System.Reactive.Disposables;
using AngorApp.Model.Funded.Shared.Model;
using AngorApp.UI.Sections.Funded.Shared.Manage;
using AngorApp.UI.Shell;
using DynamicData;
using Zafiro.UI.Navigation;
using Zafiro.UI.Shell.Utils;

namespace AngorApp.UI.Sections.Funded.Shared.Section
{
    [Section("Funded", icon: "fa-arrow-trend-up", sortIndex: 3)]
    [SectionGroup("INVESTOR")]
    public class FundedSectionViewModel : IFundedSectionViewModel, IDisposable
    {
        private readonly CompositeDisposable disposables = new();

        public FundedSectionViewModel(
            IShellViewModel shell,
            IWalletPortfolio walletPortfolio,
            INavigator navigator
        )
        {
            FindProjects = EnhancedCommand.Create(() => shell.SetSection("Find Projects"));

            walletPortfolio.InvestmentChanges
                .Transform(funded =>
                {
                    var manage = EnhancedCommand.Create(() => navigator.Go(() => new ManageViewModel(funded)));
                    return (IFundedItem)new FundedItem(funded, manage);
                })
                .DisposeMany()
                .SortBy(item => item.Funded.InvestorData.InvestedOn == DateTimeOffset.MinValue
                    ? long.MaxValue
                    : -item.Funded.InvestorData.InvestedOn.UtcTicks)
                .Bind(out var fundedItems)
                .Subscribe()
                .DisposeWith(disposables);

            FundedItems = fundedItems;
            Refresh = walletPortfolio.RefreshInvestments;
        }

        public IEnhancedCommand FindProjects { get; }
        public IReadOnlyCollection<IFundedItem> FundedItems { get; }
        public IEnhancedCommand Refresh { get; }

        public void Dispose()
        {
            FindProjects.Dispose();
            disposables.Dispose();
        }
    }
}
