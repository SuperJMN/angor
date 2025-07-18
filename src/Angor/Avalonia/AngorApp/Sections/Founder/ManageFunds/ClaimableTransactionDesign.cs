namespace AngorApp.Sections.Founder.ManageFunds;

public class ClaimableTransactionDesign : IClaimableTransaction
{
    public IAmountUI Amount { get; set; } = new AmountUI(100000); 
    public string Address { get; set; }  = "bc1qexampleaddress"; 
    public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Unspent;
}