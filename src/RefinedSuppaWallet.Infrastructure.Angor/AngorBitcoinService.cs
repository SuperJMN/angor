using Angor.Client.Shared;
using Angor.Client.Storage;
using Angor.Shared;
using Angor.Shared.Models;
using Blockcore.Consensus.ScriptInfo;
using Blockcore.NBitcoin;
using Blockcore.Networks;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using FeeRate = RefinedSuppaWallet.Domain.FeeRate;
using ScriptType = RefinedSuppaWallet.Domain.ScriptType;
using Wallet = RefinedSuppaWallet.Domain.Wallet;

public class WalletDerivationService : IWalletDerivationService
{
    private readonly Network _network;

    public WalletDerivationService(INetworkConfiguration networkConfiguration)
    {
        _network = networkConfiguration.GetNetwork();
    }

    public AccountInfo BuildAccountInfoFromDescriptor(WalletDescriptor descriptor)
    {
        var accountInfo = new AccountInfo
        {
            ExtPubKey = descriptor.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Value,
            Path = "m/84'/0'/0'", // Path estándar para SegWit
            AddressesInfo = new List<AddressInfo>(),
            ChangeAddressesInfo = new List<AddressInfo>()
        };

        // Derivar direcciones de recepción
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
        // Usar NBitcoin para parsear y derivar desde el descriptor
        // Por ejemplo, para un descriptor wpkh([fp/84'/0'/0']xpub.../0/*)
        var parsedDescriptor = ParseDescriptor(descriptor);
        
        for (int i = startIndex; i < startIndex + count; i++)
        {
            var script = DeriveScriptPubKeyFromDescriptor(descriptor, i);
            yield return script.GetDestinationAddress(_network).ToString();
        }
    }

    public Script DeriveScriptPubKeyFromDescriptor(string descriptor, int index)
    {
        // Implementar la derivación basada en el tipo de descriptor
        if (descriptor.StartsWith("wpkh("))
        {
            return DeriveSegWitScript(descriptor, index);
        }
        else if (descriptor.StartsWith("tr("))
        {
            return DeriveTaprootScript(descriptor, index);
        }

        throw new NotSupportedException($"Descriptor type not supported: {descriptor}");
    }

    private Script DeriveSegWitScript(string descriptor, int index)
    {
        // Implementar derivación SegWit
        throw new NotImplementedException();
    }

    private Script DeriveTaprootScript(string descriptor, int index)
    {
        // Implementar derivación Taproot
        throw new NotImplementedException();
    }

    private object ParseDescriptor(string descriptor)
    {
        // Implementar parsing de descriptores
        throw new NotImplementedException();
    }
}

public interface IWalletDerivationService
{
    AccountInfo BuildAccountInfoFromDescriptor(WalletDescriptor descriptor);
    IEnumerable<string> DeriveAddressesFromDescriptor(string descriptor, int startIndex, int count);
    Script DeriveScriptPubKeyFromDescriptor(string descriptor, int index);
}


public class BitcoinTransactionService : IBitcoinTransactionService
{
    private readonly IWalletOperations _walletOperations;
    private readonly IWalletDerivationService _derivationService;
    private readonly INetworkConfiguration _networkConfiguration;
    private readonly IClientStorage _clientStorage;
    private readonly PasswordComponent _passwordComponent;

    public BitcoinTransactionService(
        IWalletOperations walletOperations,
        IWalletDerivationService derivationService,
        INetworkConfiguration networkConfiguration,
        IClientStorage clientStorage,
        PasswordComponent passwordComponent)
    {
        _walletOperations = walletOperations;
        _derivationService = derivationService;
        _networkConfiguration = networkConfiguration;
        _clientStorage = clientStorage;
        _passwordComponent = passwordComponent;
    }

    public async Task<Result<Fee>> EstimateFee(Wallet wallet, Amount amount, Address address, FeeRate feeRate)
    {
        try
        {
            var accountInfo = _derivationService.BuildAccountInfoFromDescriptor(wallet.Descriptor);
            
            var changeAddress = accountInfo.GetNextChangeReceiveAddress();
            if (changeAddress == null)
            {
                return Result.Failure<Fee>("No change address available");
            }

            var sendInfo = new SendInfo
            {
                SendToAddress = address.Value,
                SendAmount = Money.Satoshis(amount.Value).ToUnit(MoneyUnit.BTC),
                FeeRate = Money.Satoshis(feeRate.Value).ToUnit(MoneyUnit.BTC),
                ChangeAddress = changeAddress
            };

            var feeSatoshis = _walletOperations.CalculateTransactionFee(
                sendInfo,
                accountInfo,
                feeRate.Value);

            return Result.Success(new Fee((long)feeSatoshis));
        }
        catch (Exception ex)
        {
            return Result.Failure<Fee>($"Error estimating fee: {ex.Message}");
        }
    }

    public async Task<Result<TxId>> PrepareAndBroadcast(
        Wallet wallet, 
        SendRequest sendRequest, 
        Address address, 
        FeeRate feeRate)
    {
        try
        {
            // Verificar que tenemos la contraseña
            if (!_passwordComponent.HasPassword())
            {
                return Result.Failure<TxId>("Wallet password required");
            }

            var accountInfo = _derivationService.BuildAccountInfoFromDescriptor(wallet.Descriptor);

            var changeAddress = accountInfo.GetNextChangeReceiveAddress();
            if (changeAddress == null)
            {
                return Result.Failure<TxId>("No change address available");
            }

            var sendInfo = new SendInfo
            {
                SendToAddress = address.Value,
                SendAmount = Money.Satoshis(sendRequest.Amount.Value).ToUnit(MoneyUnit.BTC),
                SendFee = Money.Satoshis(sendRequest.Fee.Value).ToUnit(MoneyUnit.BTC),
                FeeRate = Money.Satoshis(feeRate.Value).ToUnit(MoneyUnit.BTC),
                ChangeAddress = changeAddress
            };

            var utxosWithPath = _walletOperations.FindOutputsForTransaction(
                sendRequest.Amount.Value,
                accountInfo);

            if (!utxosWithPath.Any())
            {
                return Result.Failure<TxId>("Insufficient funds or no available UTXOs");
            }

            sendInfo.SendUtxos = utxosWithPath.ToDictionary(
                utxo => $"{utxo.UtxoData.outpoint.transactionId}:{utxo.UtxoData.outpoint.outputIndex}",
                utxo => utxo);

            // Obtener las palabras de la cartera usando el componente de contraseña
            var walletWords = await _passwordComponent.GetWalletAsync();
            
            var result = await _walletOperations.SendAmountToAddress(
                walletWords,
                sendInfo);

            if (!result.Success)
                return Result.Failure<TxId>(result.Message ?? "Failed to send transaction");

            var spentUtxos = _walletOperations.UpdateAccountUnconfirmedInfoWithSpentTransaction(
                accountInfo, 
                result.Data);

            return Result.Success(new TxId(result.Data.GetHash().ToString()));
        }
        catch (Exception ex)
        {
            return Result.Failure<TxId>($"Error preparing or broadcasting transaction: {ex.Message}");
        }
        finally
        {
            // Opcional: Limpiar la contraseña después de usar
            _passwordComponent.ClearPassword();
        }
    }
}

// Extensión del PasswordComponent para manejar el resultado
public static class PasswordComponentExtensions
{
    public static async Task<Result<WalletWords>> GetWalletWordsWithResult(this PasswordComponent passwordComponent)
    {
        try
        {
            if (!passwordComponent.HasPassword())
            {
                return Result.Failure<WalletWords>("Password not available");
            }

            var words = await passwordComponent.GetWalletAsync();
            return Result.Success(words);
        }
        catch (Exception ex)
        {
            return Result.Failure<WalletWords>($"Error getting wallet words: {ex.Message}");
        }
    }
}