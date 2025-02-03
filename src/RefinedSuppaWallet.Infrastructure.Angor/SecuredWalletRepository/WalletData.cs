using RefinedSuppaWallet.Domain;

namespace RefinedSuppaWallet.Infrastructure.Angor.SecuredWalletRepository;

public class WalletData
{
    public List<StoredWallet> Wallets { get; set; } = new();

    public class StoredWallet
    {
        public Guid Id { get; set; }
        public string Fingerprint { get; set; }
        public BitcoinNetwork Network { get; set; }
        public List<StoredXPub> XPubs { get; set; } = new();
    }

    public class StoredXPub
    {
        public string Value { get; set; }
        public ScriptType ScriptType { get; set; }
        public StoredDerivationPath Path { get; set; }
    }

    public class StoredDerivationPath
    {
        public uint Purpose { get; set; }
        public uint CoinType { get; set; }
        public uint Account { get; set; }
    }

    public class StoredTransaction
    {
        public string Id { get; set; }
        public long Balance { get; set; }
        public List<StoredTransactionInput> WalletInputs { get; set; } = new();
        public List<StoredTransactionOutput> WalletOutputs { get; set; } = new();
        public List<StoredAddressInfo> AllInputs { get; set; } = new();
        public List<StoredAddressInfo> AllOutputs { get; set; } = new();
        public ulong Fee { get; set; }
        public bool IsConfirmed { get; set; }
        public int? BlockHeight { get; set; }
        public DateTimeOffset? BlockTime { get; set; }
        public string RawJson { get; set; }
    }

    public class StoredTransactionInput
    {
        public StoredAddressInfo Address { get; set; }
        public string TxId { get; set; }
        public int Index { get; set; }
    }

    public class StoredTransactionOutput
    {
        public StoredAddressInfo Address { get; set; }
        public int Index { get; set; }
    }

    public class StoredAddressInfo
    {
        public string Address { get; set; }
        public ulong TotalAmount { get; set; }
    }
}