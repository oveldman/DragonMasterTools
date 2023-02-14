using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Serilog;

namespace DragonMaster.Build;

public static class AzureHelper
{
    public static async Task Publish(string artifactDirectory, string appName, string appUser, string appPassword)
    {
        var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{appUser}:{appPassword}"));
        using var memStream = new MemoryStream(File.ReadAllBytes( $"{artifactDirectory}/deployment.zip"));
        
        memStream.Position = 0;
        var content = new StreamContent(memStream);
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
        var requestUrl = $"https://{appName}.scm.azurewebsites.net/api/zipdeploy";
        var response = await httpClient.PostAsync(requestUrl, content);

        if (response.IsSuccessStatusCode)
        {
            Log.Information("Deployment {AppName} is successful deployed", appName);
        }
        else
        {
            Assert.Fail($"Deployment {appName} returned status code: {response.StatusCode}");
        }
    }
}