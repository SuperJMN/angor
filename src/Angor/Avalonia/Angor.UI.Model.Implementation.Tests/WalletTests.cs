using Angor.Client.Shared;
using Angor.Shared;
using Angor.Shared.Networks;
using Angor.UI.Model.Implementation.Tests.Logging;
using Blockcore.Networks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RefinedSuppaWallet.Domain;
using RefinedSuppaWallet.Infrastructure.Angor;
using Xunit.Abstractions;

namespace Angor.UI.Model.Implementation.Tests;

public class WalletApplicationServiceTests
{
    private readonly ILoggerFactory loggerFactory;

    public WalletApplicationServiceTests(ITestOutputHelper output)
    {
        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddXUnitLogger(output);
        });   
    }
    
    [Fact]
    public async Task Send()
    {
        var indexerService = DependencyFactory.GetIndexerService(NullLoggerFactory.Instance);
        var networkConfiguration = new NetworkConfiguration();
        networkConfiguration.SetNetwork(new BitcoinTest4());
        
        var walletOperations = new WalletOperations(indexerService, new HdOperations(), loggerFactory.CreateLogger<WalletOperations>(), networkConfiguration);
        var walletDerivationService = new WalletDerivationService(networkConfiguration);
        var sut = new AngorBitcoinTransactionService(walletOperations, walletDerivationService, new TestingPasswordComponent());
        
        var result = await sut.EstimateFee(SampleData.Wallet(), new Amount(1000), new Address("tb1qussl74dx2dqhrxnh89sw74v7he262vmynp7c9v"), new FeeRate(1200));
        
        Assert.True(result.IsSuccess);
    }
}