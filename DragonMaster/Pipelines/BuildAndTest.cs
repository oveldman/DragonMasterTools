using System;
using System.IO;
using System.IO.Compression;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

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
    InvokedTargets = new[] { nameof(PublishBlazor) })]
[GitHubActions(
    "PublishAPIs",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishApis) })]
public class BuildAndTest : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<BuildAndTest>(
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

    Target Clean => _ => _
        .Executes(() =>
        {
            DotNetClean(_ => _
                .SetProject(Solution));

            if (Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, true);
                Console.WriteLine($"Delete folder: {OutputDirectory}");
            }
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
                .SetOutput($"{OutputDirectory}/UI/Output"));
        });
    
    Target ZipBlazorArtifacts => _ => _
        .DependsOn(CreateBlazorArtifacts)
        .Executes(() =>
        {
            ZipFile.CreateFromDirectory($"{OutputDirectory}/{BlazorSubDirectory}/Output", $"{OutputDirectory}/{BlazorSubDirectory}/deployment.zip");
            Console.WriteLine("Blazor deployment.zip created");
        });
    
    Target PublishBlazor => _ => _
        .DependsOn(ZipBlazorArtifacts)
        .Executes(async () =>
        {
            // https://github.com/Azure/azure-sdk-for-net/blob/Azure.ResourceManager_1.4.0/sdk/resourcemanager/Azure.ResourceManager/README.md
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
            Console.WriteLine("Azure Function Anonymous deployment.zip created");
            ZipFile.CreateFromDirectory($"{OutputDirectory}/{AuthorizedSubDirectory}/Output", $"{OutputDirectory}/{AuthorizedSubDirectory}/deployment.zip");
            Console.WriteLine("Azure Function Authorized deployment.zip created");
        });
    
    Target PublishApis => _ => _
        .DependsOn(ZipApiArtifacts)
        .Executes(async () =>
        {
            // https://github.com/Azure/azure-sdk-for-net/blob/Azure.ResourceManager_1.4.0/sdk/resourcemanager/Azure.ResourceManager/README.md
        });
}
