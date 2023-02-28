using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using DragonMaster.API.Domain.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace DragonMaster.API.Authorized.Test;

public static class Ping
{
    [Authorize]
    [OpenApiOperation(operationId: "Run", tags: new []{ SwaggerGroups.Test })]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")] 
    [Function(nameof(Ping))]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, HttpMethods.Get)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var isAuthenticated = ClaimsPrincipal.Current?.Identity?.IsAuthenticated ?? false;
        Console.WriteLine("Is authenticated: " + isAuthenticated);
        
        var logger = executionContext.GetLogger("Ping");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString("Pong");

        return response;
        
    }
}