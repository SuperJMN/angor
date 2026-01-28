namespace AngorApp.UI.Flows.InvestV2
{
    public class FooterViewModel : IFooterViewModel
    {
        public IAmountUI AmountToInvest { get; } = AmountUI.FromBtc(0.4m);
        public int NumberOfReleases { get; } = 1;
        public IEnhancedCommand Invest { get; }
    }
}