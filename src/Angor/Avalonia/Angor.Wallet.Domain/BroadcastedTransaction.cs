namespace SuppaWallet.Domain;

public record TransactionAddressInfo(
    string Address,
    ulong TotalAmount // Satoshis asociados a esta dirección en la transacción
);

public record TransactionInputInfo(
    TransactionAddressInfo Address,
    string TxId, // Transacción padre
    int Index // Índice del output gastado
);

public record TransactionOutputInfo(
    TransactionAddressInfo Address,
    int Index // Posición en los outputs
);

public record BroadcastedTransaction(
    Balance Balance,
    string Id,
    IEnumerable<TransactionInputInfo> WalletInputs, // Inputs de nuestra cartera
    IEnumerable<TransactionOutputInfo> WalletOutputs, // Outputs a nuestra cartera
    IEnumerable<TransactionAddressInfo> AllInputs, // Todas las direcciones de inputs
    IEnumerable<TransactionAddressInfo> AllOutputs, // Todas las direcciones de outputs
    ulong Fee,
    bool IsConfirmed,
    int? BlockHeight,
    DateTimeOffset? BlockTime,
    string RawJson
);