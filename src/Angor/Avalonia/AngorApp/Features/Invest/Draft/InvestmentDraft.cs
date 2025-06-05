using System.Threading.Tasks;
using Angor.Contexts.Funding.Investor;
using Angor.Contexts.Funding.Investor.Operations;
using Angor.Contexts.Funding.Projects.Domain;

namespace AngorApp.Features.Invest.Draft;

public class InvestmentDraft(IInvestmentAppService investmentAppService, IWallet wallet, IProject project,  CreateInvestmentDraft.Draft draftModel) : IInvestmentDraft
{
    public CreateInvestmentDraft.Draft DraftModel { get; } = draftModel;

    public AmountUI TotalFee => new AmountUI(DraftModel.TotalFee.Sats);

    public Task<Result<Guid>> Confirm()
    {
        return investmentAppService.RequestInvestment(wallet.Id.Value, new ProjectId(project.Id), DraftModel);
    }
}