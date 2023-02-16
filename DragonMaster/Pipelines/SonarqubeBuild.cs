using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tools.SonarScanner;

namespace DragonMaster.Build;

[GitHubActions(
    "Sonarqube",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] {"main"},
    InvokedTargets = new[] {nameof(SonarQube)},
    ImportSecrets = new[] {nameof(SonarToken)})]
public partial class Build
{
    private const string RequiredFramework = "net5.0";
    
    [Parameter] [Secret] readonly string SonarToken;

    Target SonarQube => _ => _
        .DependsOn(Clean)
        .Triggers(SonarScannerStart)
        .Triggers(Compile)
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
}