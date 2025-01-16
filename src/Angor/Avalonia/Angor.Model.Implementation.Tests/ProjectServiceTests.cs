using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Angor.Model.Implementation.Projects;
using Angor.Shared.Models;
using Angor.Shared.Services;
using Angor.Test.Suppa;
using DynamicData;
using Microsoft.Extensions.Logging;
using Nostr.Client.Client;
using Xunit.Abstractions;

namespace Angor.Model.Implementation.Tests;

public class ProjectServiceTests
{
    private readonly ILoggerFactory loggerFactory;

    public ProjectServiceTests(ITestOutputHelper output)
    {
        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddXUnitLogger(output);
        });
    }

    [Fact]
    public async Task GetProjectsFromService()
    {
        var service = new ProjectService(DependencyFactory.GetIndexerService(loggerFactory), DependencyFactory.GetRelayService(loggerFactory));

        var projectsList = await service.Connect()
            .ToCollection()
            .Where(projects => projects.Any())
            .Take(1) 
            .ToTask();

        Assert.NotEmpty(projectsList);
    }
}