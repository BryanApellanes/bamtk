using Bam.Command;
using Bam.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam
{
    public class CompositeCommandContextResolver : CommandContextResolver
    {
        public CompositeCommandContextResolver(IMenuManager menuManager, MenuCommandRunner menuCommandRunner, ProcessCommandRunner processCommandRunner) : this(new List<IBrokeredCommandContextResolver>() 
            {
                new MenuCommandContextResolver(menuManager, menuCommandRunner),
                new ProcessCommandContextResolver(processCommandRunner),
            })
        {
        }

        public CompositeCommandContextResolver(IList<IBrokeredCommandContextResolver> commandContextResolvers) : base()
        {
            this.CommandContextResolvers = commandContextResolvers;
        }

        protected IList<IBrokeredCommandContextResolver> CommandContextResolvers
        {
            get;
            private set;
        }

        public override IDictionary<string, IBrokeredCommandContext> LoadContexts()
        {
            Dictionary<string, IBrokeredCommandContext> results = new Dictionary<string, IBrokeredCommandContext>();
            foreach(IBrokeredCommandContextResolver resolver in CommandContextResolvers)
            {
                IDictionary<string, IBrokeredCommandContext> next = resolver.LoadContexts();
                foreach (string key in next.Keys)
                {
                    results.Add(key, next[key]);
                }
            }

            return results;
        }
    }
}
