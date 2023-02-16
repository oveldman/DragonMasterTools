using System.IO.Compression;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DragonMaster.Build;

[GitHubActions(
    "PublishAPIs",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishApis) },
    ImportSecrets = new[] { nameof(AnonymousApiPassword), nameof(AuthorizedApiPassword) })]
public partial class Build
{
    const string AnonymousSubDirectory = "Anonymous";
    const string AuthorizedSubDirectory = "Authorized";
    
    [Parameter] readonly string AnonymousApiUser;
    [Parameter] readonly string AuthorizedApiUser;
    
    [Parameter] [Secret] readonly string AnonymousApiPassword;
    [Parameter] [Secret] readonly string AuthorizedApiPassword;
    
    Target CreateApiArtifacts => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.API.Anonymous"))
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{AnonymousSubDirectory}/Output"));
            
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.API.Authorized"))
                .SetConfiguration(Configuration)
                .EnableNoRestore()
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