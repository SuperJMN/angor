using System.Threading.Tasks;
using Angor.Contexts.Funding.Investor.Operations;

namespace AngorApp.Features.Invest.Draft;

public interface IInvestmentDraft
{
    CreateInvestmentDraft.Draft DraftModel { get; }
    AmountUI TotalFee { get; }
    Task<Result<Guid>> Confirm();
}