using System.Reactive.Linq;
using AngorApp.Sections.Wallet.Operate;
using CSharpFunctionalExtensions;
using SuppaWallet.Domain;
using SuppaWallet.Gui.Model;

namespace AngorApp.UI.Controls.Common.TransactionPreview;

public class TransactionPreviewViewModelDesign : ITransactionPreviewViewModel
{
    public IUnsignedTransaction Transaction { get; } = new UnsignedTransactionDesign()
    {
        TotalFee = 10,
        Address = "Test Address",
        Amount = 200,
        Path = "PATH",
        FeeRate = 12,
        UtxoCount = 1,
        ViewRawJson = "JSON"
    };
    public IObservable<bool> IsBusy { get; set; } = Observable.Return(false);
    public ReactiveCommand<Unit, Result<TxId>> Confirm { get; }
    public ReactiveCommand<Unit, Result<IUnsignedTransaction>> CreateTransaction { get; }
    public IObservable<bool> TransactionConfirmed { get; }
    public Destination Destination { get; } = new("Sample Destination", 1000, "mzHrLAR3WWLE4eCpq82BDCKmLeYRyYXPtm");
    public double Feerate { get; set; } = 1;
    public IObservable<bool> IsValid { get; }
    public bool AutoAdvance => false;
}