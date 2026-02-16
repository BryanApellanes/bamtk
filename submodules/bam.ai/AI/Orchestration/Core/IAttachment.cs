using System.IO;

namespace Bam.AI.Orchestration.Core
{
    /// <summary>
    /// Represents a file or data attachment to a prompt
    /// </summary>
    public interface IAttachment
    {
        string Name { get; }
        string MimeType { get; }
        Stream Content { get; }
        long Size { get; }
    }
}