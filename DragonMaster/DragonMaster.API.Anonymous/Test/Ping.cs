using System.Collections.Generic;
using System.Net;
using DragonMaster.API.Domain.API;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace DragonMaster.API.Anonymous.Test;

public static class Ping
{
    [OpenApiOperation(operationId: "Run", tags: new []{ SwaggerGroups.Test })]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")] 
    [Function(nameof(Ping))]
    public static HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, HttpMethods.Get)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("PingTrigger");
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        response.WriteString("Pong");

        return response;
        
    }
}