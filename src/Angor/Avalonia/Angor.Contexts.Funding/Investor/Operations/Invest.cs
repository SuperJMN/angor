using Angor.Contexts.Funding.Projects.Domain;
using Angor.Contexts.Funding.Shared;
using Angor.Contexts.Wallet.Infrastructure.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Angor.Contexts.Funding.Investor.Operations;

public static class Invest
{
    public class Request(Guid walletId, ProjectId projectId) : IRequest<Result>
    {
        public Guid WalletId { get; } = walletId;
        public ProjectId ProjectId { get; } = projectId;
    }

    public class Handler(IProjectRepository projectRepository, ISeedwordsProvider seedwordsProvider)
        : IRequestHandler<Request, Result>
    {
        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var result = await from project in projectRepository.Get(request.ProjectId)
                from words in seedwordsProvider.GetSensitiveData(request.WalletId)
                select new { project, words };

            return Result.Success();
        }
    }
}