using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DotnetPackaging.Deployment;
using DotnetPackaging.Deployment.Core;
using DotnetPackaging.Deployment.Platforms.Android;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Serilog;
using Zafiro.DivineBytes;

[AzurePipelines(
    AzurePipelinesImage.UbuntuLatest,
    InvokedTargets = [nameof(Publish)], AutoGenerate = true,
    ImportSecrets = ["GitHubApiKey"],
    FetchDepth = 0, ImportVariableGroups = ["api-keys"])
]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;
    [GitRepository] readonly GitRepository Repository;

    [Parameter("Force publish even if not on main/master branch")] readonly bool Force;
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter] [Secret] readonly string GitHubApiKey;
    [Parameter] [Secret] readonly string AndroidBase64Keystore;
    [Parameter] [Secret] readonly string AndroidSigningKeyAlias;
    [Parameter] [Secret] readonly string AndroidSigningStorePass;
    [Parameter] [Secret] readonly string AndroidSigningKeyPass;
    
    [Parameter("NuGet Source URL for publishing packages")] readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

    Target Publish => d => d
        .Requires(() => GitHubApiKey)
        .Requires(() => AndroidSigningStorePass)
        .Requires(() => AndroidSigningKeyAlias)
        .Requires(() => AndroidSigningKeyPass)
        .Requires(() => AndroidBase64Keystore)
        .OnlyWhenStatic(() => Repository.IsOnMainOrMasterBranch() || Force)
        .Executes(async () =>
        {
            Log.Information("Starting publish process for Angor app...");
            Log.Information("Solution: {SolutionPath}", Solution.Path);
            Log.Information("Projects: {@Projects}", Solution.GetAllProjects("*.*").Select(x => x.Name));
            
            var desktopProject = Solution.GetAllProjects("*.*").TryFirst(project => project.Name.Contains("Desktop", StringComparison.OrdinalIgnoreCase));
            var androidProject = Solution.GetAllProjects("*.*").TryFirst(project => project.Name.Contains("Android", StringComparison.OrdinalIgnoreCase));

            var projectsResult = Result.Combine(
                desktopProject.ToResult("Desktop project not found."),
                androidProject.ToResult("Android project not found.")
            );
            
            if (projectsResult.IsFailure)
            {
                Assert.Fail(projectsResult.Error);
                return;
            }
            
            var result = await CreateRelease(androidProject.Value, desktopProject.Value);
            
            result
                .TapError(err => Assert.Fail(err))
                .Tap(() => Log.Information("GitHub Release created successfully."));
        });

    private async Task<Result> CreateRelease(Project androidProject, Project desktopProject)
    {
        var gitHubRepositoryConfig = new GitHubRepositoryConfig("SuperJMN", "angor", GitHubApiKey);
        var releaseData = new ReleaseData($"v{GitVersion.MajorMinorPatch}", $"v{GitVersion.MajorMinorPatch}", "Relase body");
        
        Log.Information("Desktop Project: {DesktopProject}", desktopProject.Name);

        var androidOptions = new AndroidDeployment.DeploymentOptions
        {
            SigningKeyAlias =  AndroidSigningKeyAlias,
            ApplicationDisplayVersion = GitVersion.MajorMinorPatch,
            ApplicationVersion = int.Parse(GitVersion.MajorMinorPatch.Replace(".", "")),
            PackageName = "Angor",
            SigningKeyPass = AndroidSigningKeyPass,
            AndroidSigningKeyStore = ByteSource.FromBytes(Convert.FromBase64String(AndroidBase64Keystore)),
            SigningStorePass = AndroidSigningStorePass
        };
        
        var release = Deployer.Instance.CreateRelease()
            .WithApplicationInfo("Angor", "io.angor.angorapp", "Angor")
            .WithVersion(GitVersion.MajorMinorPatch)
            .ForLinux(desktopProject.Path)
            .ForWindows(desktopProject.Path)
            .ForAndroid(androidProject.Path, androidOptions)
            .Build();
                    
        return await Deployer.Instance.CreateGitHubRelease(release, gitHubRepositoryConfig, releaseData);
    }
}