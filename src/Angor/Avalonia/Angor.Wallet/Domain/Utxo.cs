namespace SuppaWallet.Domain;

public record Utxo(TxId TxId, OutputIndex OutputIndex, Amount Balance, Address Address, string ScriptPubKeyHex, DerivationType DerivationType, int DerivationIndex);