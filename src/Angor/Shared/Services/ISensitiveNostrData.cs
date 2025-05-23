using CSharpFunctionalExtensions;

namespace Angor.Shared.Services;

public interface ISensitiveNostrData
{
    Result<string> GetNostrPrivateKey(KeyIdentifier keyIdentifier);
}