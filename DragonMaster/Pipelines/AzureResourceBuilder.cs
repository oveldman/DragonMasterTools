using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using Serilog;

namespace DragonMaster.Build;

public class AzureResourceBuilder
{
    private readonly string ResourceGroupName;
    private readonly string SubscriptionId;
    private readonly ArmClient ArmClient;

    public AzureResourceBuilder(
        string subscriptionId, 
        string resourceGroupName, 
        string tenantId, 
        string clientId, 
        string clientSecret)
    {
        ResourceGroupName = resourceGroupName;
        SubscriptionId = subscriptionId;
        ArmClient = BuildClient(subscriptionId, tenantId, clientId, clientSecret);
    }

    public async Task DeployTemplate(string pathToTemplate, string deploymentName)
    {
        var subscription = await ArmClient.GetDefaultSubscriptionAsync();
        var resourceGroup = (await subscription
                .GetResourceGroups()
                .CreateOrUpdateAsync(WaitUntil.Completed, ResourceGroupName, new ResourceGroupData(AzureLocation.WestEurope)))
            .Value;

        var deploymentContent = GetTemplate(pathToTemplate);

        await resourceGroup.GetArmDeployments()
            .CreateOrUpdateAsync(WaitUntil.Completed, deploymentName, deploymentContent);
        
        Log.Information("ARM template {DeploymentName} is successful deployed", deploymentName);
    }

    public ArmDeploymentValidateResult ValidateTemplate(string pathToTemplate, string deploymentName)
    {
        var deploymentContent = GetTemplate(pathToTemplate);
        
        var resourceIdentifier = new ResourceIdentifier($"/subscriptions/{SubscriptionId}/resourceGroups/DragonMaster/providers/Microsoft.Resources/deployments/{deploymentName}");
        var response = ArmClient
            .GetArmDeploymentResource(resourceIdentifier)
            .Validate(WaitUntil.Completed, deploymentContent);

        return response.Value;
    }

    private static ArmClient BuildClient(string subscriptionId, string tenantId, string clientId, string clientSecret)
    {
        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        return new ArmClient(credential, subscriptionId);
    }

    private static ArmDeploymentContent GetTemplate(string pathToTemplate)
    {
        var templateContent = File.ReadAllText(pathToTemplate).TrimEnd();
        return  new ArmDeploymentContent(new ArmDeploymentProperties(ArmDeploymentMode.Incremental)
        {
            Template = BinaryData.FromString(templateContent)
        });
    }
}