using Angor.UI.Model.Wallet;
using Angor.Wallet.Domain;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls.Wizards.Builder;

namespace AngorApp.UI.Controls.Common.TransactionPreview;

public interface ITransactionPreviewViewModel : IStep
{
    public IUnsignedTransaction Transaction { get; }
    ReactiveCommand<Unit, Result<TxId>> Confirm { get; }
    ReactiveCommand<Unit, Result<IUnsignedTransaction>> CreateTransaction { get; }
    public IObservable<bool> TransactionConfirmed { get; }
    public Destination Destination { get; }
    public double Feerate { get; set; }
}