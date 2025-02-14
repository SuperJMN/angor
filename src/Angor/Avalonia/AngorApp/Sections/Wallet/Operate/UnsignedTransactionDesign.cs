﻿using System.Threading.Tasks;
using Angor.UI.Model.Wallet;
using Angor.Wallet.Domain;
using CSharpFunctionalExtensions;

namespace AngorApp.Sections.Wallet.Operate;

public class UnsignedTransactionDesign : IUnsignedTransaction
{
    public string Address { get; set; }
    
    public long FeeRate { get; set; }

    public Task<Result<TxId>> Accept()
    {
        return Task.FromResult(new Result<TxId>());
    }

    public long TotalFee { get; set; }
    public long Amount { get; set; }
    public string Path { get; set; }
    public int UtxoCount { get; set; }
    public string ViewRawJson { get; set; }
}