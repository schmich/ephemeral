using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Ephemeral.Commands
{
    public abstract class CommandFactory
    {
        public CommandFactory(ICommandManager manager)
        {
        }
    }

    public interface ICommandManager
    {
        void AddCommand(Command c);
        void RemoveCommand(Command c);
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ExportAttribute : Attribute
    {
        public ExportAttribute()
        {
        }
    }

    public class SingleCommandFactory : CommandFactory
    {
        public SingleCommandFactory(ICommandManager manager, Command command)
            : base(manager)
        {
            manager.AddCommand(command);
        }
    }

    public class SingleCommandFactory<T> : SingleCommandFactory
        where T : Command, new()
    {
        public SingleCommandFactory(ICommandManager controller)
            : base(controller, new T())
        {
        }
    }
}
