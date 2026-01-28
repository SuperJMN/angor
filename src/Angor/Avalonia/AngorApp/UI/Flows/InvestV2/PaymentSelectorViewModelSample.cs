namespace AngorApp
{
    public class PaymentSelectorViewModelSample : IPaymentSelectorViewModel
    {
        public IAmountUI AmountToInvest { get; } = AmountUI.FromBtc(0.5m);
        public IEnumerable<IWallet> Wallets { get; }
        public IWallet SelectedWallet { get; set; }
    }
}