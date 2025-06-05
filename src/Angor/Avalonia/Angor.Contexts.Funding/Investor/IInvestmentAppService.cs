using Angor.Contexts.Funding.Founder.Operations;
using Angor.Contexts.Funding.Investor.Dtos;
using Angor.Contexts.Funding.Investor.Operations;
using Angor.Contexts.Funding.Projects.Domain;
using CSharpFunctionalExtensions;
using Investment = Angor.Contexts.Funding.Founder.Operations.Investment;

namespace Angor.Contexts.Funding.Investor;

public interface IInvestmentAppService
{
    Task<Result<CreateInvestmentDraft.Draft>> CreateInvestmentRequestDraft(Guid sourceWalletId, ProjectId projectId, Amount amount);
    Task<Result<Guid>> RequestInvestment(Guid sourceWalletId, ProjectId projectId, CreateInvestmentDraft.Draft draft);
    Task<Result<IEnumerable<Investment>>> GetInvestments(Guid walletId, ProjectId projectId);
    Task<Result> ApproveInvestmentRequest(Guid walletId, ProjectId projectId, Investment investment);
    Task<Result> Invest(int requestId);
}