using Angor.Shared.Models;
using Blockcore.Consensus.ScriptInfo;
using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public interface IWalletDerivationService
{
    AccountInfo BuildAccountInfoFromDescriptor(WalletDescriptor descriptor);
    IEnumerable<string> DeriveAddressesFromDescriptor(string descriptor, int startIndex, int count);
    Script DeriveScriptPubKeyFromDescriptor(string descriptor, int index);
}