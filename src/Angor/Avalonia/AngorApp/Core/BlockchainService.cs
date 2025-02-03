using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using RefinedSuppaWalet.Infrastructure.Interfaces;
using RefinedSuppaWalet.Infrastructure.Interfaces.Transaction;
using RefinedSuppaWalet.Infrastructure.Interfaces.Wallet;
using RefinedSuppaWallet.Application;
using RefinedSuppaWallet.Domain;

namespace AngorApp.Core;

internal class BlockchainService : IBlockchainService
{
    private readonly IUtxoRepository utxoRepository;
    private readonly IBitcoinTransactionService transactionService;
    private readonly IWalletTransactionService transactionRepository;
    private readonly ITransactionBroadcaster broadcaster;

    public BlockchainService(IUtxoRepository utxoRepository, IBitcoinTransactionService transactionService, IWalletTransactionService transactionRepository, ITransactionBroadcaster broadcaster)
    {
        this.utxoRepository = utxoRepository;
        this.transactionService = transactionService;
        this.transactionRepository = transactionRepository;
        this.broadcaster = broadcaster;
    }

    public Task<Result<IEnumerable<Utxo>>> GetUtxos(Wallet wallet)
    {
        return utxoRepository.GetUtxos(wallet);
    }

    public Task<Result<IEnumerable<BroadcastedTransaction>>> GetTransactions(Wallet wallet)
    {
        return transactionRepository.GetWalletTransactions(wallet.Descriptor);
    }

    public Task<Result<UnsignedTransaction>> CreateTransaction(Wallet wallet, Amount amount, Address address, FeeRate feeRate)
    {
        return transactionService.PrepareAndBroadcast(wallet, amount, address, feeRate);
    }

    public Task<Result<TxId>> Broadcast(SignedTransaction signedTransaction)
    {
        return broadcaster.Broadcast(signedTransaction);
    }
}