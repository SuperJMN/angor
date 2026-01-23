using System.Windows.Input;
using AngorApp.Model.Contracts.Amounts;
using AngorApp.UI.Flows.Invest.Amount;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace AngorApp.UI.Flows.InvestV2;

public class InvestViewModelSample : ReactiveValidationObject, IInvestViewModel
{
    public InvestViewModelSample()
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
        
        Invest = ReactiveCommand.Create(() => { });
        Cancel = ReactiveCommand.Create(() => { });
        SelectAmount = ReactiveCommand.Create<long>(a => Amount = a);
    }

    public long? Amount { get; set; }
    public IEnumerable<Breakdown> StageBreakdowns { get; }
    public TransactionDetails? TransactionDetails { get; }
    public ICommand Invest { get; }
    public ICommand Cancel { get; }
    public ICommand SelectAmount { get; }
    public string ProjectName { get; }
    public string ProjectId { get; }
    public IObservable<bool> IsValid => Observable.Return(true);
}
