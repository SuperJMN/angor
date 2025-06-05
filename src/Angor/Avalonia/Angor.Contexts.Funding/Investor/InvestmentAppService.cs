using Angor.Contexts.Funding.Founder.Operations;
using Angor.Contexts.Funding.Investor.Dtos;
using Angor.Contexts.Funding.Investor.Operations;
using Angor.Contexts.Funding.Projects.Domain;
using CSharpFunctionalExtensions;
using MediatR;
using Investment = Angor.Contexts.Funding.Founder.Operations.Investment;

namespace Angor.Contexts.Funding.Investor;

public class InvestmentAppService(IInvestmentRepository investmentRepository, IMediator mediator) : IInvestmentAppService 
{
    public Task<Result<CreateInvestmentDraft.Draft>> CreateInvestmentRequestDraft(Guid sourceWalletId, ProjectId projectId, Amount amount)
    {
        return mediator.Send(new CreateInvestmentDraft.CreateInvestmentTransactionRequest(sourceWalletId, projectId, amount));
    }

    public Task<Result<Guid>> RequestInvestment(Guid sourceWalletId, ProjectId projectId, CreateInvestmentDraft.Draft draft)
    {
        return mediator.Send(new RequestInvestment.RequestFounderSignaturesRequest(sourceWalletId, projectId, draft));
    }

    public Task<Result<IEnumerable<Investment>>> GetInvestments(Guid walletId, ProjectId projectId)
    {
        return mediator.Send(new GetInvestments.GetInvestmentsRequest(walletId, projectId));
    }

    public Task<Result> ApproveInvestmentRequest(Guid walletId, ProjectId projectId, Investment investment)
    {
        return mediator.Send(new ApproveInvestmentRequest.Request(walletId, projectId, investment));
    }

    public Task<Result> Invest(int requestId)
    {
        return mediator.Send(new Invest.Request(requestId));
    }
}