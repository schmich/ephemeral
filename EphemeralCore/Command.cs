using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;

namespace Ephemeral.Commands
{
    public interface ICommand
    {
        string Name { get; }

        Bitmap Icon { get; }

        void Execute(string arguments);
    }
}
