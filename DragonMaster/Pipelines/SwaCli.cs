using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.Tooling;
using Serilog;

namespace DragonMaster.Build;

public class SwaCli
{
    private readonly Tool Cli;

    private string Environment = string.Empty;
    private string Token = string.Empty;
    private string ApplicationLocation = string.Empty;

    private readonly static IReadOnlyList<string> CommonDebugLogs = new List<string>()
    {
        "Preparing deployment. Please wait..",
        "Project deployed to https://",
        "Downloading https://swalocaldeploy.azureedge.net/downloads/"
    };

    private SwaCli()
    {
        Cli = ToolResolver.GetPathTool("swa");
    }
    
    public static SwaCli Create()
    {
        return new SwaCli();
    }
    
    public SwaCli WithApplicationLocation(string applicationLocation)
    {
        ApplicationLocation = $"--app-location {applicationLocation}";
        return this;
    }
    
    public SwaCli WithToken(string token)
    {
        Token = $"--deployment-token {token}";
        return this;
    }
    
    public SwaCli WithEnvironment(string environment)
    {
        Environment = $"--env {environment}";
        return this;
    }

    public void Execute()
    {
        var command = BuildCommand();
        Cli(command, customLogger: SwaLogger);
    }

    private string BuildCommand()
    {
        return $"deploy {ApplicationLocation} {Token} {Environment}";
    }

    private readonly static Action<OutputType, string> SwaLogger = (outputType, text) =>
    {
        if (outputType == OutputType.Std || IsDebugLog(text))
        {
            Log.Debug(text);
            return;
        }
        
        Log.Error(text);
    };

    private static bool IsDebugLog(string log)
    {
        return CommonDebugLogs.Any(log.Contains);
    }
}