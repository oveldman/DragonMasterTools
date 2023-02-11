using DragonMaster.Web.Application.Data.Test;
using DragonMaster.Web.Infrastructure.ApiServices.Test;

namespace DragonMaster.Web.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDragonMaster(this IServiceCollection services)
    {
        services.AddScoped<IPingService, PingService>();
        return services;
    }
}