using Bam.Command;
using Bam.Net;

namespace Bam.Tools;

public class ToolsProcessCommandContextResolver : ProcessCommandContextResolver
{
    public ToolsProcessCommandContextResolver(ProcessCommandRunner commandRunner) : base(commandRunner)
    {
    }

    public static string ToolsPath => Path.Combine(Environment.CurrentDirectory, ".bam", "tools");
    
    protected override void SetDirectories()
    {
        List<DirectoryInfo> searchDirectories = new List<DirectoryInfo>();
        searchDirectories.Add(new DirectoryInfo(ToolsPath));
     
        base.SetDirectories();
        if (this.SearchDirectories.Length > 0)
        {
            searchDirectories.AddRange(this.SearchDirectories);
        }
        this.SearchDirectories = searchDirectories.ToArray();
    }

    protected override bool IsValidCommand(string path)
    {
        if (path.ToLowerInvariant().StartsWith(BamProfile.ToolkitPath))
        {
            return base.IsValidCommand(path);
        }

        return true;
    }
}