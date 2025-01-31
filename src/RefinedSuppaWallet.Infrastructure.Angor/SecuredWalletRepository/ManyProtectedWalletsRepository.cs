using MoreLinq;
using RefinedSuppaWalet.Infrastructure.Interfaces.Wallet;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor.Store;
using System.Text.Json;
using CSharpFunctionalExtensions;

namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class AngorWalletRepository : IProtectedWalletRepository
{
    private const string WalletsFile = "wallets.json";
    private readonly IStore store;
    private readonly IWalletTransactionService transactionService;
    private readonly ManyWalletsData allWallets;

    public static async Task<IProtectedWalletRepository> Create(IWalletTransactionService transactionService, IStore store)
    {
        var wallets = await LoadWallets(store);
        return new AngorWalletRepository(wallets, store, transactionService);
    }

    private AngorWalletRepository(ManyWalletsData allWallets, IStore store, IWalletTransactionService transactionService)
    {
        this.allWallets = allWallets;
        this.store = store;
        this.transactionService = transactionService;
    }

    private static async Task<ManyWalletsData> LoadWallets(IStore store)
    {
        var data = await store.Load<ManyWalletsData>(WalletsFile);
        return data ?? new ManyWalletsData();
    }

    private async Task SaveWallets()
    {
        await store.Save(WalletsFile, allWallets);
    }

    public async Task<IEnumerable<(WalletId Id, string Name)>> ListWallets()
    {
        return allWallets.Wallets.Select(x => (new WalletId(x.WalletId), x.WalletName));
    }

    public async Task<Maybe<Wallet>> Get(WalletId id, string passphrase)
    {
        var info = allWallets.Wallets.FirstOrDefault(x => x.WalletId == id.Id);
        if (info == null)
            return Maybe<Wallet>.None;

        try
        {
            var json = WalletCrypto.Decrypt(info.EncryptedData, info.Salt, passphrase);
            var storedWallet = JsonSerializer.Deserialize<WalletData.StoredWallet>(json);
            if (storedWallet == null)
                return Maybe<Wallet>.None;

            return Maybe<Wallet>.From(MapToWallet(storedWallet));
        }
        catch
        {
            // passphrase incorrecta o datos corruptos
            return Maybe<Wallet>.None;
        }
    }

    public async Task<Result<Wallet>> ImportWallet(WalletId walletId, string walletName, WalletDescriptor descriptor, string passphrase)
    {
        var exists = allWallets.Wallets.Any(w => w.WalletId == walletId.Id);
        if (exists)
            return Result.Failure<Wallet>("Wallet already exists.");

        var domainWallet = new Wallet(walletId, descriptor);

        var addTransactionsResult = await AddTransactions(domainWallet);
        if (addTransactionsResult.IsFailure)
        {
            return Result.Failure<Wallet>("Cannot import wallet. Error fetching transactions.");
        }

        var storedWallet = MapToStoredWallet(domainWallet);

        // serializamos la info sensible y la ciframos
        var json = JsonSerializer.Serialize(storedWallet);
        var (encrypted, salt) = WalletCrypto.Encrypt(json, passphrase);

        // creamos la entrada
        var newEntry = new EncryptedWalletInfo
        {
            WalletId = walletId.Id,
            WalletName = walletName, // en claro
            EncryptedData = encrypted, // cifrado
            Salt = salt
        };

        // añadimos a la lista y guardamos
        allWallets.Wallets.Add(newEntry);
        await SaveWallets();

        return Result.Success(domainWallet);
    }

    private Task<Result> AddTransactions(Wallet domainWallet)
    {
        return transactionService.GetWalletTransactions(domainWallet.Descriptor)
            .Tap(txs => txs.ForEach(txt => domainWallet.RegisterBroadcastedTransaction(txt)))
            .Bind(_ => Result.Success());
    }

    private static Wallet MapToWallet(WalletData.StoredWallet stored)
    {
        var descriptor = WalletDescriptor.Create(
            stored.Fingerprint,
            stored.Network,
            XPubCollection.Create(
                XPub.Create(
                    stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Value,
                    ScriptType.SegWit,
                    DerivationPath.Create(
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.Purpose,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.CoinType,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.SegWit).Path.Account
                    )
                ),
                XPub.Create(
                    stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Value,
                    ScriptType.Taproot,
                    DerivationPath.Create(
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.Purpose,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.CoinType,
                        stored.XPubs.First(x => x.ScriptType == ScriptType.Taproot).Path.Account
                    )
                )
            ));

        var wallet = new Domain.Wallet(new WalletId(stored.Id), descriptor);

        foreach (var tx in stored.Transactions)
        {
            wallet.AddIncomingTransaction(new BroadcastedTransaction(
                new Balance(tx.Balance),
                tx.Id,
                tx.WalletInputs.Select(i => new TransactionInputInfo(
                    new TransactionAddressInfo(i.Address.Address, i.Address.TotalAmount),
                    i.TxId,
                    i.Index
                )),
                tx.WalletOutputs.Select(o => new TransactionOutputInfo(
                    new TransactionAddressInfo(o.Address.Address, o.Address.TotalAmount),
                    o.Index
                )),
                tx.AllInputs.Select(i => new TransactionAddressInfo(i.Address, i.TotalAmount)),
                tx.AllOutputs.Select(o => new TransactionAddressInfo(o.Address, o.TotalAmount)),
                tx.Fee,
                tx.IsConfirmed,
                tx.BlockHeight,
                tx.BlockTime,
                tx.RawJson
            ));
        }

        return wallet;
    }

    private static WalletData.StoredWallet MapToStoredWallet(Domain.Wallet wallet)
    {
        return new WalletData.StoredWallet
        {
            Id = wallet.Id.Id,
            Fingerprint = wallet.Descriptor.Fingerprint,
            Network = wallet.Descriptor.Network,
            XPubs = wallet.Descriptor.XPubs.Select(x => new WalletData.StoredXPub
            {
                Value = x.Value,
                ScriptType = x.ScriptType,
                Path = new WalletData.StoredDerivationPath
                {
                    Purpose = x.Path.Purpose,
                    CoinType = x.Path.CoinType,
                    Account = x.Path.Account
                }
            }).ToList(),
            Transactions = wallet.Transactions.Select(tx => new WalletData.StoredTransaction
            {
                Id = tx.Id,
                Balance = tx.Balance.Value,
                WalletInputs = tx.WalletInputs.Select(i => new WalletData.StoredTransactionInput
                {
                    Address = new WalletData.StoredAddressInfo
                    {
                        Address = i.Address.Address,
                        TotalAmount = i.Address.TotalAmount
                    },
                    TxId = i.TxId,
                    Index = i.Index
                }).ToList(),
                WalletOutputs = tx.WalletOutputs.Select(o => new WalletData.StoredTransactionOutput
                {
                    Address = new WalletData.StoredAddressInfo
                    {
                        Address = o.Address.Address,
                        TotalAmount = o.Address.TotalAmount
                    },
                    Index = o.Index
                }).ToList(),
                AllInputs = tx.AllInputs.Select(i => new WalletData.StoredAddressInfo
                {
                    Address = i.Address,
                    TotalAmount = i.TotalAmount
                }).ToList(),
                AllOutputs = tx.AllOutputs.Select(o => new WalletData.StoredAddressInfo
                {
                    Address = o.Address,
                    TotalAmount = o.TotalAmount
                }).ToList(),
                Fee = tx.Fee,
                IsConfirmed = tx.IsConfirmed,
                BlockHeight = tx.BlockHeight,
                BlockTime = tx.BlockTime,
                RawJson = tx.RawJson
            }).ToList()
        };
    }
}