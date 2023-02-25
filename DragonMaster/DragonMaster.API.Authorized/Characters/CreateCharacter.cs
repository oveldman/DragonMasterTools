using System.Collections.Generic;
using System.Net;
using DragonMaster.API.Domain.API;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DragonMaster.API.Authorized.Characters;

public static class CreateCharacter
{
    [Function(nameof(CreateCharacter))]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, HttpMethods.Post)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("CreateCharacter");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString("Welcome to Azure Functions!");

        return response;
        
    }
}