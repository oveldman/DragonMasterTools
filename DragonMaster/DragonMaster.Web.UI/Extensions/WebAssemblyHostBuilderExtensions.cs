using DragonMaster.Web.Application.Authorization;
using DragonMaster.Web.Domain.Api;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace DragonMaster.Web.UI.Extensions;

public static class WebAssemblyHostBuilderExtensions
{
    public static WebAssemblyHostBuilder AddHttpClients(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        
        builder.AddAnonymousHttpClient();
        builder.AddAuthorizedHttpClient();

        return builder;
    }
    
    public static WebAssemblyHostBuilder AddDragonMaster(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddDragonMaster();
        builder.AddConfigurationSettings();
        return builder;
    }
    
    private static void AddAnonymousHttpClient(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddHttpClient(ApiTypes.DragonMasterApiAnonymous, (serviceProvider, client) =>
        {
            var apiUrls = serviceProvider.GetService<ApiUrls>()!;
            client.BaseAddress = new Uri(apiUrls.Anonymous);
        }).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    }
    
    private static void AddAuthorizedHttpClient(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped<DragonAuthorizationMessageHandler>();
        builder.Services.AddHttpClient(ApiTypes.DragonMasterApiAuthorized, (serviceProvider, client) =>
        {
            var apiUrls = serviceProvider.GetService<ApiUrls>()!;
            client.BaseAddress = new Uri(apiUrls.Authorized);
        }).AddHttpMessageHandler<DragonAuthorizationMessageHandler>();
    }

    private static void AddConfigurationSettings(this WebAssemblyHostBuilder builder)
    {
        var apiUrls = builder
            .Configuration
            .GetSection("ApiUrls")
            .Get<ApiUrls>()!;
        
        builder.Services.AddSingleton(apiUrls);
    }
}