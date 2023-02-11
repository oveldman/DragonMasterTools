using DragonMaster.Web.Application.Data.Test;
using Microsoft.AspNetCore.Components;

namespace DragonMaster.Web.UI.Pages.Test;

public partial class Ping
{
    [Inject] public IPingService PingService { get; set; } = default!; 
    
    private string _anonymousPingResponse = string.Empty;
    private string _authorizedPingResponse = string.Empty;
    
    public async Task GetAnonymousPingAsync()
    {
        _anonymousPingResponse = await PingService.GetAnonymousPing();
    }
    
    public async Task GetAuthorizedPingAsync()
    {
        _authorizedPingResponse = await PingService.GetAuthorizedPing();
    }
}