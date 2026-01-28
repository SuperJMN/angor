namespace AngorApp.UI.Flows.InvestV2
{
    public class PaymentSelectorViewModel : IPaymentSelectorViewModel, IHaveTitle
    {
        public IAmountUI AmountToInvest { get; } = AmountUI.FromBtc(0.5m);
        public IEnumerable<IWallet> Wallets { get; }
        public IWallet SelectedWallet { get; set; }
        public IObservable<string> Title { get; } = Observable.Return("Select Payment Method");
    }
}