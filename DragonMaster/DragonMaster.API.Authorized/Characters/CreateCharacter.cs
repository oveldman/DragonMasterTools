using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using DragonMaster.API.Application.UseCases.Characters;
using DragonMaster.API.Domain.API;
using DragonMaster.API.Infrastructure.AzureFunctions;
using DragonMaster.Contracts.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DragonMaster.API.Authorized.Characters;

public class CreateCharacter
{
    private readonly CreateCharacterUseCase _useCase;

    public CreateCharacter(CreateCharacterUseCase useCase)
    {
        _useCase = useCase;
    }
    
    [Authorize]
    [OpenApiOperation(operationId: "Run", tags: new []{ SwaggerGroups.Characters })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CreateCharacterRequest), Required = true, Description = "Create a new character")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
    [Function(nameof(CreateCharacter))]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, HttpMethods.Post)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger(nameof(CreateCharacter));
        logger.LogInformation("C# HTTP trigger function processed a request.");
        
        var requestBody = await req.GetBodyAsync<CreateCharacterRequest>();
        _useCase.CreateCharacter(requestBody);
        
        var response = req.CreateResponse(HttpStatusCode.OK);
        return response;
    }
}