using Nuke.Common.Tooling;

namespace DragonMaster.Build;

public static class SwaCli
{
    public static Tool Create()
    { 
        return ToolResolver.GetPathTool("swa");
    } 
}