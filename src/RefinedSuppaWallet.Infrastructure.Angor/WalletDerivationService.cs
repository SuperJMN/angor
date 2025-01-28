using Angor.Shared;
using Angor.Shared.Models;
using RefinedSuppaWallet.Infrastructure.Angor;
using Network = Blockcore.Networks.Network;
using Script = Blockcore.Consensus.ScriptInfo.Script;
using Blockcore.NBitcoin.DataEncoders;
using NBitcoin;
using RefinedSuppaWallet.Domain;
using PubKey = Blockcore.NBitcoin.PubKey;
using ScriptType = RefinedSuppaWallet.Domain.ScriptType;

public class WalletDerivationService : IWalletDerivationService
{
    private readonly Network network;

    public WalletDerivationService(INetworkConfiguration networkConfiguration)
    {
        network = networkConfiguration.GetNetwork();
    }

    public AccountInfo BuildAccountInfoFromDescriptor(WalletDescriptor descriptor)
    {
        var accountInfo = new AccountInfo
        {
            ExtPubKey = descriptor.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Value,
            Path = descriptor.XPubs.First().Path.ToString(),
            AddressesInfo = new List<AddressInfo>(),
            ChangeAddressesInfo = new List<AddressInfo>()
        };

        foreach (var xpub in descriptor.XPubs)
        {
            var receiveDescriptor = xpub.ToDescriptor(DerivationType.Receive);
            var changeDescriptor = xpub.ToDescriptor(DerivationType.Change);

            var receiveAddresses = DeriveAddressesFromDescriptor(receiveDescriptor, 0, 20)
                .Select(addr => new AddressInfo { Address = addr, UtxoData = new List<UtxoData>() });
            var changeAddresses = DeriveAddressesFromDescriptor(changeDescriptor, 0, 20)
                .Select(addr => new AddressInfo { Address = addr, UtxoData = new List<UtxoData>() });

            accountInfo.AddressesInfo.AddRange(receiveAddresses);
            accountInfo.ChangeAddressesInfo.AddRange(changeAddresses);
        }

        return accountInfo;
    }
    public IEnumerable<string> DeriveAddressesFromDescriptor(string descriptor, int startIndex, int count)
    {
        var pubKey = ParsePubKeyFromDescriptor(descriptor);
        var scriptPubKeyType = GetScriptPubKeyTypeFromDescriptor(descriptor);

        for (int i = startIndex; i < startIndex + count; i++)
        {
            var script = scriptPubKeyType switch
            {
                ScriptPubKeyType.Segwit => pubKey.WitHash.ScriptPubKey,
                ScriptPubKeyType.TaprootBIP86 => Script.FromHex("5120" + pubKey.ToHex()), // Taproot P2TR
                _ => throw new NotSupportedException($"Script type not supported")
            };
            
            var address = script.GetDestinationAddress(network).ToString();
            yield return address;
        }
    }

    public Script DeriveScriptPubKeyFromDescriptor(string descriptor, int index)
    {
        var pubKey = ParsePubKeyFromDescriptor(descriptor);
        var scriptPubKeyType = GetScriptPubKeyTypeFromDescriptor(descriptor);

        return scriptPubKeyType switch
        {
            ScriptPubKeyType.Segwit => pubKey.WitHash.ScriptPubKey,
            ScriptPubKeyType.TaprootBIP86 => Script.FromHex("5120" + pubKey.ToHex()),
            _ => throw new NotSupportedException($"Script type not supported")
        };
    }

    private PubKey ParsePubKeyFromDescriptor(string descriptor)
    {
        int start = descriptor.LastIndexOf('(') + 1;
        int end = descriptor.IndexOf('/', start);
        if (end == -1) end = descriptor.IndexOf(')', start);
    
        var xpubStr = descriptor.Substring(start, end - start);
        if (xpubStr.Contains('[')) 
        {
            xpubStr = xpubStr.Substring(xpubStr.IndexOf(']') + 1);
        }
    
        // Convertir xpub a ExtPubKey y obtener la clave pública
        var extPubKey = Blockcore.NBitcoin.BIP32.ExtPubKey.Parse(xpubStr, network);
        return extPubKey.PubKey;
    }

    private ScriptPubKeyType GetScriptPubKeyTypeFromDescriptor(string descriptor)
    {
        if (descriptor.StartsWith("wpkh("))
            return ScriptPubKeyType.Segwit;
        if (descriptor.StartsWith("tr("))
            return ScriptPubKeyType.TaprootBIP86;
        
        throw new NotSupportedException($"Descriptor type not supported: {descriptor}");
    }
}