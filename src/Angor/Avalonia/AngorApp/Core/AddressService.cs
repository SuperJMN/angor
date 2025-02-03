using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure;
using RefinedSuppaWalet.Infrastructure.Address;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace AngorApp.Core;

internal class AddressService : IAddressService
{
    private readonly IAddressManager addressManager;

    public AddressService(IAddressManager addressManager)
    {
        this.addressManager = addressManager;
    }

    public Task<Result<Address>> GetReceiveAddress(Wallet wallet, ScriptType scriptType)
    {
        return addressManager.GetNextAddress(wallet.Descriptor, DerivationType.Receive, scriptType);
    }

    public Task<Result<Address>> GetChangeAddress(Wallet wallet, ScriptType scriptType)
    {
        return addressManager.GetNextAddress(wallet.Descriptor, DerivationType.Change, scriptType);
    }

    public Task<Result<IEnumerable<Address>>> GetAddresses(Wallet wallet)
    {
        return addressManager.ScanAddresses(wallet.Descriptor).Map(list => list.Select(info => new Address(info.Address)));
    }
}