using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;

namespace AngorApp.Core;

internal class PassphraseProvider : IPassphraseProvider
{
    public Task<Result<string>> RequestPassphrase()
    {
        throw new NotImplementedException();
    }
}