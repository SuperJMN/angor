namespace AngorApp
{
    public interface IFooterViewModel
    {
        public IAmountUI AmountToInvest { get; }
        public int NumberOfReleases { get; }
        public IEnhancedCommand Invest { get; }
    }
}