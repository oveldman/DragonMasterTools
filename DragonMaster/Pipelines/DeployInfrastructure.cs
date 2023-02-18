using System.IO;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Serilog;

namespace DragonMaster.Build;

[GitHubActions(
    "ArmTemplateDeployment",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] {"main"},
    InvokedTargets = new[] {nameof(DeployInfrastructure)},
    ImportSecrets = new[] {nameof(AzureClientId), nameof(AzureClientSecret), nameof(AzureTenantId), nameof(AzureSubscriptionId) })]
public partial class Build
{
    const string DeploymentName = "NukeDeployment";
    const string MainArmTemplate = "ArmExample.json";
    const string PipelineLocation = "DragonMaster.Resources";
    const string ResourceGroupName = "DragonMaster";
    
    [Parameter] [Secret] readonly string AzureClientId;
    [Parameter] [Secret] readonly string AzureClientSecret;
    [Parameter] [Secret] readonly string AzureTenantId;
    [Parameter] [Secret] readonly string AzureSubscriptionId;
    
    Target DeployInfrastructure => _ => _
        .DependsOn(VerifyInfrastructure)
        .Executes(async () =>
        {
            var armTemplate = Path.Combine(Solution.Path!.Parent!, PipelineLocation, MainArmTemplate);
            var resourceBuilder = new AzureResourceBuilder(
                AzureSubscriptionId, 
                ResourceGroupName,
                AzureTenantId,
                AzureClientId,
                AzureClientSecret
            );

            await resourceBuilder.DeployTemplate(armTemplate, DeploymentName);
        });
    
    Target VerifyInfrastructure => _ => _
        .Executes(() =>
        {
            var armTemplate = Path.Combine(Solution.Path!.Parent!, PipelineLocation, MainArmTemplate);
            var resourceBuilder = new AzureResourceBuilder(
                AzureSubscriptionId, 
                ResourceGroupName,
                AzureTenantId,
                AzureClientId,
                AzureClientSecret
                );
            
            var result = resourceBuilder.ValidateTemplate(armTemplate, DeploymentName);
            if (result.Error is not null)
            {
                Assert.Fail($"ARM template validation failed: {result.Error.Message}");
            }

            Log.Information("ARM template validation successful");
        });
}