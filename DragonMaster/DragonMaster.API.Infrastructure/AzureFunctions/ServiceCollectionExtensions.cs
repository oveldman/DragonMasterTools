using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace DragonMaster.API.Infrastructure.AzureFunctions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var options = new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "DragonMaster API",
                    Description = "This is a sample server DragonMaster API designed by Oscar Veldman.",
                    TermsOfService = new Uri("https://github.com/oveldman/DragonMasterTools"),
                    Contact = new OpenApiContact()
                    {
                        Name = "Oscar Veldman",
                        Email = "oveldman@gmail.com",
                        Url = new Uri("https://github.com/oveldman/DragonMasterTools/issues"),
                    },
                    License = new OpenApiLicense()
                    {
                        Name = "MIT",
                        Url = new Uri("http://opensource.org/licenses/MIT"),
                    }
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = true,
                ForceHttps = false,
                ForceHttp = false,
            };

            return options;
        });
        
        return services;
    }
}