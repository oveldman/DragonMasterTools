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
    
    private static void AddAnonymousHttpClient(this WebAssemblyHostBuilder builder)
    {
        var apiUrlAnonymous = builder.Configuration["ApiUrls:Anonymous"]!;

        builder.Services.AddHttpClient(ApiTypes.DragonMasterApiAnonymous, (serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(apiUrlAnonymous);
        }).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    }
    
    private static void AddAuthorizedHttpClient(this WebAssemblyHostBuilder builder)
    {
        var apiUrlAnonymous = builder.Configuration["ApiUrls:Authorized"]!;

        builder.Services.AddHttpClient(ApiTypes.DragonMasterApiAuthorized, (serviceProvider, client) =>
        {
            client.BaseAddress = new Uri(apiUrlAnonymous);
        }).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
    }
}