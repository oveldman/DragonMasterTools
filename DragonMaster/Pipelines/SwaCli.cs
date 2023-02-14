using System;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Serilog;

namespace DragonMaster.Build;

public class SwaCli
{
    private readonly Tool Cli;
    private static Action<OutputType, string> CustomLogger => (_, text) => Log.Debug(text);
    
    private SwaCli()
    {
        Cli = ToolResolver.GetPathTool("swa");
    }
    
    public static SwaCli Create()
    {
        return new SwaCli();
    }

    public void Execute(string command)
    {
        Cli(command, customLogger: CustomLogger);
    }
}