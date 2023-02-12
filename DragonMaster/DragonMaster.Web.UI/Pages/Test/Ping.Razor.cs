using DragonMaster.Web.Application.Data.Test;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace DragonMaster.Web.UI.Pages.Test;

public partial class Ping
{
    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] public IPingService PingService { get; set; } = default!;

    private bool Authenticated { get; set; }

    private string _anonymousPingResponse = string.Empty;
    private string _authorizedPingResponse = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var authenticated = authenticationState.User.Identity?.IsAuthenticated ?? false;
        await base.OnInitializedAsync();
    }

    private async Task GetAnonymousPingAsync()
    {
        _anonymousPingResponse = await PingService.GetAnonymousPing();
    }
    
    private async Task GetAuthorizedPingAsync()
    {
        _authorizedPingResponse = await PingService.GetAuthorizedPing();
    }
}