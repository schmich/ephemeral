using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace Ephemeral.Commands
{
    [Export(typeof(ICommandFactory))]
    class SingleCommandFactory : ICommandFactory
    {
        [ImportingConstructor]
        public SingleCommandFactory(ICommandStore store, [ImportMany] IEnumerable<ICommand> commands)
        {
            foreach (var c in commands)
                store.AddCommand(c);
        }
    }
}
