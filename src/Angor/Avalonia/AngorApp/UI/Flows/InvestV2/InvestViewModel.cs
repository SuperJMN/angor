using System.Windows.Input;
using AngorApp.UI.Flows.Invest.Amount;
using ReactiveUI.Validation.Helpers;
using Zafiro.Avalonia.Dialogs;
using Zafiro.UI.Navigation;
using Option = Zafiro.Avalonia.Dialogs.Option;

namespace AngorApp.UI.Flows.InvestV2
{
    public class InvestViewModel : ReactiveValidationObject, IInvestViewModel, IHaveFooter
    {
        public InvestViewModel(UIServices uiServices)
        {
            ProjectName = "Example Project";
            ProjectId = "b04c...8d1a";
            Amount = 100000;
        
            StageBreakdowns = new List<Breakdown>
            {
                new(1, new AmountUI(50000), 0.5m, DateTime.Now.AddDays(30)),
                new(2, new AmountUI(25000), 0.25m, DateTime.Now.AddDays(60)),
                new(3, new AmountUI(25000), 0.25m, DateTime.Now.AddDays(90))
            };
        
            TransactionDetails = new TransactionDetails(
                new AmountUI(100000),
                new AmountUI(500),
                new AmountUI(1000)
            );
        
            Invest = ReactiveCommand.Create(() => uiServices.Dialog.Show(new PaymentSelectorViewModel(), "Select Wallet", closeable => 
            [
                new Option("Choose Wallet", ReactiveCommand.Create(() => { }).Enhance(), new Settings()),
                new Option("Generate Invoice Instead", ReactiveCommand.Create(() => { }).Enhance(), new Settings())
            ]));
            
            SelectAmount = ReactiveCommand.Create<long>(a => Amount = a);
        }

        public decimal? Amount { get; set; }
        public IEnumerable<Breakdown> StageBreakdowns { get; }
        public TransactionDetails? TransactionDetails { get; }
        public ICommand Invest { get; }
        public ICommand SelectAmount { get; }
        public string ProjectName { get; }
        public string ProjectId { get; }
        public IObservable<bool> IsValid => Observable.Return(true);
        public IEnumerable<IAmountUI> AmountPresets { get; } = [AmountUI.FromBtc(0.001), AmountUI.FromBtc(0.01), AmountUI.FromBtc(0.1), AmountUI.FromBtc(0.5)];
        public IAmountUI SelectedAmountPreset { get; set; }
        public string ProjectTitle { get; } = "Angor UX";
        public decimal Progress { get; } = 0.21m;
        public IAmountUI Raised { get; } = AmountUI.FromBtc(0.1234m);
        public IAmountUI AmountToInvest { get; } = AmountUI.FromBtc(0.001m);
        public int NumberOfReleases { get; } = 3;
        public IObservable<object> Footer { get; } = Observable.Return(new FooterViewModel());
    }
}