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
        public CommandFactory(ICommandController controller)
        {
        }
    }

    public interface ICommandController
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
        public SingleCommandFactory(ICommandController controller, Command command)
            : base(controller)
        {
            controller.AddCommand(command);
        }
    }

    public class SingleCommandFactory<T> : SingleCommandFactory
        where T : Command, new()
    {
        public SingleCommandFactory(ICommandController controller)
            : base(controller, new T())
        {
        }
    }
}
