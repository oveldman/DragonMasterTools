using DragonMaster.API.Infrastructure.AzureFunctions;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => worker.UseNewtonsoftJson())
    .ConfigureServices(services =>
    {
        services.ConfigureOpenApi();
        services.ConfigureDragonMaster();
    })
    .Build();

host.Run();