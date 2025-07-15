using CSharpFunctionalExtensions;
using DotnetPackaging.Deployment;
using DotnetPackaging.Deployment.Core;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Serilog;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest, 
    InvokedTargets = [nameof(Publish)], AutoGenerate = true, 
    ImportSecrets = ["GitHubApiKey"],
    FetchDepth = 0, ImportVariableGroups = ["api-keys"])
]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Publish);

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository Repository;
    
    [Parameter("Force publish even if not on main/master branch")] readonly bool Force;
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter] [Secret] readonly string GitHubApiKey;
    [Parameter("NuGet Source URL for publishing packages")] readonly string NuGetSource = "https://api.nuget.org/v3/index.json";
    
    Target Publish => d => d
        .Requires(() => GitHubApiKey)
        .OnlyWhenStatic(() => Repository.IsOnMainOrMasterBranch() || Force)
        .Executes(async () =>
        {
            Log.Information("Starting publish process for Angor app...");
            Log.Information("Solution: {SolutionPath}", Solution.Path);

            var gitHubRepositoryConfig = new GitHubRepositoryConfig("SuperJMN", "angor", GitHubApiKey);
            var releaseData = new ReleaseData($"v{GitVersion.MajorMinorPatch}", $"v{GitVersion.MajorMinorPatch}", "Relase body");
            
            await Deployer.Instance.CreateAvaloniaReleaseFromSolution(Solution.Path!.ToString()!, GitVersion.MajorMinorPatch, "Angor", "io.angor.angorapp", "Angor", gitHubRepositoryConfig, releaseData)
                .TapError(err => Assert.Fail(err))
                .Tap(() => Log.Information("GitHub Release created successfully."));
        });
}