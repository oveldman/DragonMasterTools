using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.SonarScanner;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace DragonMaster.Build;

[GitHubActions(
    "Sonarqube",
    GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    InvokedTargets = new[] {nameof(SonarQube)},
    ImportSecrets = new[] {nameof(SonarToken)})]
public partial class Build
{
    const string RequiredFramework = "net5.0";
    
    [Parameter] [Secret] readonly string SonarToken;

    Target SonarQube => _ => _
        .DependsOn(Clean)
        .Triggers(SonarScannerStart)
        .Triggers(Compile)
        .Triggers(TestAndCollect)
        .Triggers(SonarScannerEnd);

    Target SonarScannerStart => _ => _
        .Before(Compile)
        .Executes(() =>
        {
            var settings = new SonarScannerBeginSettings()
                .SetFramework(RequiredFramework)
                .SetProjectKey("oveldman_DragonMasterTools")
                .SetOrganization("oveldman")
                .SetLogin(SonarToken)
                .SetServer("https://sonarcloud.io");

            SonarScannerTasks.SonarScannerBegin(settings);
        });
    
    Target SonarScannerEnd => _ => _
        .After(Compile)
        .Executes(() =>
        {
            var settings = new SonarScannerEndSettings()
                .SetFramework(RequiredFramework)
                .SetLogin(SonarToken);
            
            SonarScannerTasks.SonarScannerEnd(settings);
        });
    
    Target TestAndCollect => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild()
                .SetCoverletOutput("XPlat Code Coverage")
                .SetCoverletOutputFormat("opencover")
                .SetResultsDirectory("TestResults/")
            );
        });
}