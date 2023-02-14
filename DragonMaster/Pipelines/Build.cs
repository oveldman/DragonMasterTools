using System.IO.Compression;
using Microsoft.Build.Utilities;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DragonMaster.Build;

[GitHubActions(
    "Build&Test",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] { nameof(Test) })]
[GitHubActions(
    "PublishBlazor",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishBlazor) },
    ImportSecrets = new[] { nameof(BlazorPublishToken) })]
[GitHubActions(
    "PublishAPIs",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishApis) },
    ImportSecrets = new[] { nameof(AnonymousApiPassword), nameof(AuthorizedApiPassword) })]
public class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(
        x => x.Test,
        x => x.PublishBlazor,
        x => x.PublishApis
    );
    
    const string OutputDirectory = "artifacts";
    const string AnonymousSubDirectory = "Anonymous";
    const string AuthorizedSubDirectory = "Authorized";
    const string BlazorSubDirectory = "UI";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Solution] readonly Solution Solution;
    
    [Parameter] readonly string AnonymousApiUser;
    [Parameter] readonly string AuthorizedApiUser;

    [Parameter] [Secret] readonly string BlazorPublishToken;
    [Parameter] [Secret] readonly string AnonymousApiPassword;
    [Parameter] [Secret] readonly string AuthorizedApiPassword;

    Target Clean => _ => _
        .Executes(() =>
        {
            DotNetClean(_ => _
                .SetProject(Solution));
            
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild());
        });

    Target CreateBlazorArtifacts => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.Web.UI"))
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{BlazorSubDirectory}/Output"));
        });

    Target InstallSwaTools => _ => _
        .DependsOn(CreateBlazorArtifacts)
        .Executes(() =>
        {
            NpmTasks.NpmInstall(_ => _
                .EnableGlobal()
                .SetPackages("@azure/static-web-apps-cli"));
        });

    Target PublishBlazor => _ => _
        .DependsOn(InstallSwaTools)
        .Executes(() =>
        {
            // https://azure.github.io/static-web-apps-cli/docs/use/deploy
            var command =
                "deploy " +
                $"--app-location {OutputDirectory}/{BlazorSubDirectory}/Output/wwwroot " +
                $"--deployment-token {BlazorPublishToken} " +
                "--env production";

            var swa = SwaCli.Create();
            swa.Execute(command);
        });

    Target CreateApiArtifacts => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.API.Anonymous"))
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{AnonymousSubDirectory}/Output"));
            
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.API.Authorized"))
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{AuthorizedSubDirectory}/Output"));
        });
    
    Target ZipApiArtifacts => _ => _
        .DependsOn(CreateApiArtifacts)
        .Executes(() =>
        {
            ZipFile.CreateFromDirectory($"{OutputDirectory}/{AnonymousSubDirectory}/Output", $"{OutputDirectory}/{AnonymousSubDirectory}/deployment.zip");
            Log.Information("Azure Function Anonymous deployment.zip created");
            ZipFile.CreateFromDirectory($"{OutputDirectory}/{AuthorizedSubDirectory}/Output", $"{OutputDirectory}/{AuthorizedSubDirectory}/deployment.zip");
            Log.Information("Azure Function Authorized deployment.zip created");
        });
    
    Target PublishApis => _ => _
        .DependsOn(ZipApiArtifacts)
        .Executes(async () =>
        {
            await AzureHelper.Publish($"{OutputDirectory}/{AnonymousSubDirectory}", "dragonmaster-Anonymous", AnonymousApiUser, AnonymousApiPassword);
            await AzureHelper.Publish($"{OutputDirectory}/{AuthorizedSubDirectory}", "dragonmaster-Authorized", AuthorizedApiUser, AuthorizedApiPassword);
        });
}