using System.Windows.Input;
using AngorApp.Model.Contracts.Amounts;
using AngorApp.UI.Flows.Invest.Amount;

namespace AngorApp.UI.Flows.InvestV2;

public record TransactionDetails(IAmountUI AmountToInvest, IAmountUI MinerFee, IAmountUI AngorFee)
{
    public IAmountUI Total => new AmountUI(AmountToInvest.Sats + MinerFee.Sats + AngorFee.Sats);
}

public interface IInvestViewModel
{
    long? Amount { get; set; }
    
    IEnumerable<Breakdown> StageBreakdowns { get; }
    
    TransactionDetails? TransactionDetails { get; }
    
    ICommand Invest { get; }
    
    ICommand Cancel { get; }
    
    ICommand SelectAmount { get; }
    
    string ProjectName { get; }
    
    string ProjectId { get; }
    
    // Validation
    IObservable<bool> IsValid { get; }
}
