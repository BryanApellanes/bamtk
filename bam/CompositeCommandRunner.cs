using Bam.Command;

namespace Bam
{
    public class CompositeCommandRunner : IBrokeredCommandRunner
    {
        public IBrokeredCommandRunResult Run(IBrokeredCommand command, string[] arguments)
        {
            throw new NotImplementedException();
        }
    }
}
