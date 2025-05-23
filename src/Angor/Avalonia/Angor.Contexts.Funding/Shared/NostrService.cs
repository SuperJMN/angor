using System.Reactive.Linq;
using Angor.Shared.Services;
using CSharpFunctionalExtensions;
using Nostr.Client.Client;
using Nostr.Client.Messages;
using Nostr.Client.Requests;
using Nostr.Client.Responses;

namespace Angor.Contexts.Funding.Shared;

public class NostrService : INostrService
{
    private readonly INostrClient nostrClient;

    public NostrService(INostrClient nostrClient)
    {
        this.nostrClient = nostrClient;
    }

    public async Task<Result<NostrOkResponse>> Send(NostrEvent nostrEvent)
    {
        try
        {
            var okStream = nostrClient
                .Streams.OkStream
                .Where(x => x.EventId == nostrEvent.Id)
                .Replay(1);

            using var connection = okStream.Connect();

            nostrClient.Send(new NostrEventRequest(nostrEvent));

            var respuesta = await okStream
                .FirstAsync()
                .Timeout(TimeSpan.FromSeconds(10));

            return Result.Success(respuesta);
        }
        catch (Exception ex)
        {
            return Result.Failure<NostrOkResponse>(ex.Message);
        }
    }
}