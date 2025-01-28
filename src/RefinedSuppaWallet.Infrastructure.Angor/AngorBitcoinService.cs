using Angor.Shared;
using Angor.Shared.Models;
using Blockcore.NBitcoin;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWallet.Domain;
using FeeRate = RefinedSuppaWallet.Domain.FeeRate;
using Wallet = RefinedSuppaWallet.Domain.Wallet;

namespace RefinedSuppaWallet.Infrastructure.Angor;

public class AngorBitcoinTransactionService : IBitcoinTransactionService
{
    private readonly IWalletOperations walletOperations;
    private readonly IWalletDerivationService derivationService;
    private readonly IPasswordComponent passwordComponent;

    public AngorBitcoinTransactionService(
        IWalletOperations walletOperations,
        IWalletDerivationService derivationService,
        IPasswordComponent passwordComponent)
    {
        this.walletOperations = walletOperations;
        this.derivationService = derivationService;
        this.passwordComponent = passwordComponent;
    }

    public async Task<Result<Fee>> EstimateFee(Wallet wallet, Amount amount, Address address, FeeRate feeRate)
    {
        // try
        // {
            var accountInfo = derivationService.BuildAccountInfoFromDescriptor(wallet.Descriptor);

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

            var feeSatoshis = walletOperations.CalculateTransactionFee(
                sendInfo,
                accountInfo,
                feeRate.Value);

            return Result.Success(new Fee((long)feeSatoshis));
        // }
        // catch (Exception ex)
        // {
        //     return Result.Failure<Fee>($"Error estimating fee: {ex.Message}");
        // }
    }

    public async Task<Result<TxId>> PrepareAndBroadcast(
        Wallet wallet,
        SendRequest sendRequest,
        Address address,
        FeeRate feeRate)
    {
        try
        {
            if (!passwordComponent.HasPassword())
                return Result.Failure<TxId>("Wallet password required");

            var accountInfo = derivationService.BuildAccountInfoFromDescriptor(wallet.Descriptor);
            var changeAddress = accountInfo.GetNextChangeReceiveAddress();
            if (changeAddress == null)
                return Result.Failure<TxId>("No change address available");

            var sendInfo = new SendInfo
            {
                SendToAddress = address.Value,
                SendAmount = Money.Satoshis(sendRequest.Amount.Value).ToUnit(MoneyUnit.BTC),
                SendFee = Money.Satoshis(sendRequest.Fee.Value).ToUnit(MoneyUnit.BTC),
                FeeRate = Money.Satoshis(feeRate.Value).ToUnit(MoneyUnit.BTC),
                ChangeAddress = changeAddress
            };

            var utxosWithPath = walletOperations.FindOutputsForTransaction(
                sendRequest.Amount.Value,
                accountInfo);

            if (!utxosWithPath.Any())
                return Result.Failure<TxId>("Insufficient funds or no available UTXOs");

            sendInfo.SendUtxos = utxosWithPath.ToDictionary(
                utxo => $"{utxo.UtxoData.outpoint.transactionId}:{utxo.UtxoData.outpoint.outputIndex}",
                utxo => utxo);

            var walletWords = await passwordComponent.GetWalletAsync();
            var result = await walletOperations.SendAmountToAddress(
                walletWords,
                sendInfo);

            if (!result.Success)
                return Result.Failure<TxId>(result.Message ?? "Failed to send transaction");

            return Result.Success(new TxId(result.Data.GetHash().ToString()));
        }
        catch (Exception ex)
        {
            return Result.Failure<TxId>($"Error preparing or broadcasting transaction: {ex.Message}");
        }
    }
}