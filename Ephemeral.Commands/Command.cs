using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;

namespace Ephemeral.Commands
{
    [DebuggerDisplay("{Name,nq}")]
    public abstract class Command
    {
        public abstract string Name { get; }

        public virtual Bitmap Icon
        {
            get { return null; }
        }

        public abstract void Execute(string arguments);

        public override string ToString()
        {
            return Name;
        }
    }
}
