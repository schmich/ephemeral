using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Ephemeral.Commands
{
    public interface ICommandFactory
    {
    }

    public interface ICommandStore
    {
        void AddCommand(ICommand c);
        void RemoveCommand(ICommand c);
    }
}
