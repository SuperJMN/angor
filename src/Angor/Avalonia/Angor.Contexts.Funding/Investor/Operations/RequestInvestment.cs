using Angor.Client.Services;
using Angor.Contests.CrossCutting;
using Angor.Contexts.Funding.Projects.Domain;
using Angor.Contexts.Funding.Shared;
using Angor.Shared;
using Angor.Shared.Models;
using Angor.Shared.Services;
using Blockcore.Consensus.TransactionInfo;
using Blockcore.NBitcoin;
using Blockcore.NBitcoin.DataEncoders;
using CSharpFunctionalExtensions;
using MediatR;

namespace Angor.Contexts.Funding.Investor.Operations;

public static class RequestInvestment
{
    public class RequestFounderSignaturesHandler(
        IProjectRepository projectRepository,
        ISeedwordsProvider seedwordsProvider,
        IDerivationOperations derivationOperations,
        IEncryptionService encryptionService,
        INetworkConfiguration networkConfiguration,
        ISerializer serializer,
        IWalletOperations walletOperations,
        ISignService signService) : IRequestHandler<RequestFounderSignaturesRequest, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RequestFounderSignaturesRequest request, CancellationToken cancellationToken)
        {
            var txnHex = request.Draft.SignedTxHex;
            var network = networkConfiguration.GetNetwork();
            var strippedInvestmentTransaction = network.CreateTransaction(txnHex);
            strippedInvestmentTransaction.Inputs.ForEach(f => f.WitScript = WitScript.Empty);

            var projectResult = await projectRepository.Get(request.ProjectId);

            if (projectResult.IsFailure)
            {
                return Result.Failure<Guid>(projectResult.Error);
            }

            var sensitiveDataResult = await seedwordsProvider.GetSensitiveData(request.WalletId);

            if (sensitiveDataResult.IsFailure)
            {
                return Result.Failure<Guid>(sensitiveDataResult.Error);
            }

            var walletWords = sensitiveDataResult.Value.ToWalletWords();
            var project = projectResult.Value;

            var sendSignatureResult = await SendSignatureRequest(request.WalletId, walletWords, project, strippedInvestmentTransaction.ToHex());

            if (sendSignatureResult.IsFailure)
            {
                return Result.Failure<Guid>(sendSignatureResult.Error);
            }

            var requestId = sendSignatureResult.Value;
            // TODO: Don't forget to uncomment. We really need to save info
            //var saveResult = await Save(requestId, txnHex, requestFounderSignaturesRequest.InvestmentTransaction.InvestorKey, requestFounderSignaturesRequest.ProjectId);
            //return saveResult.Sats;
            return Result.Success(Guid.Empty);
        }

        private async Task<Result<Guid>> Save(string requestId, string transactionHex, string investorKey, ProjectId projectId)
        {
            // TODO: Implement the save logic
            throw new NotImplementedException();
        }

        private async Task<Result<EventSendResponse>> SendSignatureRequest(Guid walletId, WalletWords walletWords, Project project, string signedTransactionHex)
        {
            try
            {
                var investorNostrPrivateKey = await derivationOperations.DeriveProjectNostrPrivateKeyAsync(walletWords, project.FounderKey);
                var investorNostrPrivateKeyHex = Encoders.Hex.EncodeData(investorNostrPrivateKey.ToBytes());
                var releaseAddressResult = await GetUnfundedReleaseAddress(walletWords);

                if (releaseAddressResult.IsFailure)
                {
                    return Result.Failure<EventSendResponse>(releaseAddressResult.Error);
                }
                
                var releaseAddress = releaseAddressResult.Value;

                var signRecoveryRequest = new SignRecoveryRequest
                {
                    ProjectIdentifier = project.Id.Value,
                    InvestmentTransactionHex = signedTransactionHex,
                    UnfundedReleaseAddress = releaseAddress,
                };

                var serializedRecoveryRequest = serializer.Serialize(signRecoveryRequest);
                
                var encryptedContent = await encryptionService.EncryptNostrContentAsync(
                    investorNostrPrivateKeyHex,
                    project.NostrPubKey,
                    serializedRecoveryRequest);

                var key = new KeyIdentifier(walletId, project.NostrPubKey);
                return await signService.PostInvestmentRequest2(key, serializer.Serialize(signRecoveryRequest),  project.NostrPubKey);

            }
            catch (Exception ex)
            {
                return Result.Failure<EventSendResponse>($"Error while sending the signature request {ex.Message}");
            }
        }

        private Task<Result<string>> GetUnfundedReleaseAddress(WalletWords wallet)
        {
            return Result.Try(async () =>
            {
                var accountInfo = walletOperations.BuildAccountInfoForWalletWords(wallet);
                await walletOperations.UpdateAccountInfoWithNewAddressesAsync(accountInfo);

                return accountInfo.GetNextReceiveAddress();
            }).EnsureNotNull("Could not get the unfunded release address");
        }
    }
    
    public class RequestFounderSignaturesRequest(Guid walletId, ProjectId projectId, CreateInvestment.Draft draft) : IRequest<Result<Guid>>
    {
        public ProjectId ProjectId { get; } = projectId;
        public CreateInvestment.Draft Draft { get; } = draft;
        public Guid WalletId { get; } = walletId;
    }
}