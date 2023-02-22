using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DragonMaster.Build;

[GitHubActions(
    "PublishBlazor",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    InvokedTargets = new[] { nameof(PublishBlazor) },
    ImportSecrets = new[] { nameof(DragonMasterBlazorPublishToken), nameof(MadWorldBlazorPublishToken) })]
public partial class Build
{
    const string DragonMasterBlazorSubDirectory = "DragonMasterUI";
    const string MadWorldBlazorSubDirectory = "MadWorldUI";
    
    [Parameter] [Secret] readonly string DragonMasterBlazorPublishToken;
    [Parameter] [Secret] readonly string MadWorldBlazorPublishToken;
    
    Target CreateBlazorArtifacts => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("DragonMaster.Web.UI"))
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{DragonMasterBlazorSubDirectory}/Output"));
            
            DotNetPublish(_ => _
                .SetProject(Solution.GetProject("MadWorld.Web.UI"))
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .SetOutput($"{OutputDirectory}/{MadWorldBlazorSubDirectory}/Output"));
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
            
            SwaCli.Create()
                .WithApplicationLocation($"{OutputDirectory}/{DragonMasterBlazorSubDirectory}/Output/wwwroot")
                .WithToken(DragonMasterBlazorPublishToken)
                .WithEnvironment("production")
                .Execute();
            
            SwaCli.Create()
                .WithApplicationLocation($"{OutputDirectory}/{MadWorldBlazorSubDirectory}/Output/wwwroot")
                .WithToken(MadWorldBlazorPublishToken)
                .WithEnvironment("production")
                .Execute();
        });
}