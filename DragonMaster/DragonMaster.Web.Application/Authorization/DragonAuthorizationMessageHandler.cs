using DragonMaster.Web.Domain.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DragonMaster.Web.Application.Authorization;

public class DragonAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public DragonAuthorizationMessageHandler(IAccessTokenProvider provider, 
        NavigationManager navigation,
        ApiUrls apiUrls)
        : base(provider, navigation)
    {
        ConfigureHandler(
            authorizedUrls: new[] { apiUrls.ApiBaseUrl },
            scopes: new[] {
                "https://nlMadWorld.onmicrosoft.com/4919e418-faad-461f-9d58-a33c0dce51f0/API.Access" 
            });
    }
}
