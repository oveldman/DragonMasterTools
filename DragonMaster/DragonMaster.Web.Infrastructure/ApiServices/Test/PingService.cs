using DragonMaster.Web.Application.Data.Test;
using DragonMaster.Web.Domain.Api;

namespace DragonMaster.Web.Infrastructure.ApiServices.Test;

public class PingService : IPingService
{
    private const string Url = "Ping";
    
    private readonly HttpClient _anonymousClient;
    private readonly HttpClient _authorizedClient;
    
    public PingService(IHttpClientFactory clientFactory)
    {
        _anonymousClient = clientFactory.CreateClient(ApiTypes.DragonMasterApiAnonymous);
        _authorizedClient = clientFactory.CreateClient(ApiTypes.DragonMasterApiAuthorized);
    }
    
    public async Task<string> GetAnonymousPing()
    {
        return await _anonymousClient.GetStringAsync(Url);
    }

    public async Task<string> GetAuthorizedPing()
    {
        return await _authorizedClient.GetStringAsync(Url);
    }
}