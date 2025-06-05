using Angor.Contexts.Funding.Projects.Domain;
using Angor.Contexts.Funding.Shared;
using Angor.Contexts.Wallet.Infrastructure.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Angor.Contexts.Funding.Investor.Operations;

public static class Invest
{
    public class Request(int requestId) : IRequest<Result>
    {
        public int RequestId { get; } = requestId;
    }

    public class Handler(IProjectRepository projectRepository, ISeedwordsProvider seedwordsProvider)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
           // we have the request id, so we can get everything for it
           throw new NotImplementedException();
        }
    }
}