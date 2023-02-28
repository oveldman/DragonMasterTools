using DragonMaster.API.Infrastructure.AzureFunctions;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder => builder.UseNewtonsoftJson())
    .ConfigureServices(services =>
    {
        services.ConfigureOpenApi();
        services.ConfigureDragonMaster();
    })
    .Build();

host.Run();