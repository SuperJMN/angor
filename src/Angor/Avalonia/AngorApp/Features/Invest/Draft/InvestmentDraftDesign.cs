using System.Threading.Tasks;
using Angor.Contexts.Funding.Investor.Operations;

namespace AngorApp.Features.Invest.Draft;

public class InvestmentDraftDesign : IInvestmentDraft
{
    public CreateInvestmentDraft.Draft DraftModel { get; }
    public AmountUI TotalFee { get; }
    public Task<Result<Guid>> Confirm()
    {
        throw new NotSupportedException();
    }
}