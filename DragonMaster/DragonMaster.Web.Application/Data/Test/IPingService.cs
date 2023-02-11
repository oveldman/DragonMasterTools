namespace DragonMaster.Web.Application.Data.Test;

public interface IPingService
{
    public Task<string> GetAnonymousPing();
    public Task<string>  GetAuthorizedPing();
}