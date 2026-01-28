namespace AngorApp
{
    public interface IPaymentSelectorViewModel
    {
        public IAmountUI AmountToInvest { get; }
        public IEnumerable<IWallet> Wallets { get; }
        IWallet SelectedWallet { get; set; }
    }
}