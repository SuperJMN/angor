namespace SuppaWallet.Domain;

public record PrevCoinData(
    string TxId,
    int Vout,
    long Amount,
    string ScriptPubKeyHex,
    DomainScriptType ScriptType,
    DerivationType DerivationType,
    int DerivationIndex
);