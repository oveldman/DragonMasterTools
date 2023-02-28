using AzureFunctions.Extensions.OpenIDConnect.Isolated.Configuration;
using DragonMaster.API.Infrastructure.AzureFunctions;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults((_, builder) =>
    {
        builder.UseNewtonsoftJson();
        builder.UseAuthorization();
    })
    .ConfigureServices((_, services) =>
    {
        services.ConfigureOpenApi();
        services.ConfigureOpenIdConnect();
        services.ConfigureDragonMaster();
    })
    .Build();

await host.RunAsync();