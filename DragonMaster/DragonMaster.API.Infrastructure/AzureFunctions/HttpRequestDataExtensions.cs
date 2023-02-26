using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;

namespace DragonMaster.API.Infrastructure.AzureFunctions;

public static class HttpRequestDataExtensions
{
    public static async Task<T?> GetBodyAsync<T>(this HttpRequestData request)
    {
        using var streamReader =  new  StreamReader(request.Body);
        var requestBody = await streamReader.ReadToEndAsync();
        return string.IsNullOrEmpty(requestBody) ? default : JsonConvert.DeserializeObject<T>(requestBody);
    } 
}