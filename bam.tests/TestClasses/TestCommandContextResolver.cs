using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Tests.TestClasses
{
    public class TestCommandContextResolver : CommandContextResolver
    {
        public TestCommandContextResolver() { }
        public TestCommandContextResolver(IBamContext context)
        {
            this.Context = context;
        }
        public IBamContext Context { get; private set; }

        public override IDictionary<string, IBrokeredCommandContext> LoadContexts()
        {
            throw new NotImplementedException();
        }
    }
}
