using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Ephemeral.Commands
{
    public static class CommandFactoryLoader
    {
        public static ICollection<CommandFactory> LoadFromAssembly(Assembly assembly, CommandController controller)
        {
            List<CommandFactory> factories = new List<CommandFactory>();

            foreach (Type type in assembly.GetTypes())
            {
                if (IsExported(type))
                {
                    if (IsCommandFactory(type))
                    {
                        factories.Add((CommandFactory)Activator.CreateInstance(type, new object[] { controller }));
                        continue;
                    }

                    bool isCommand = InheritsFrom(type, typeof(Command));
                    bool hasDefaultConstructor = type.GetConstructors().Any(c => c.GetParameters().Length == 0);
                    if (isCommand && hasDefaultConstructor)
                    {
                        factories.Add(new SingleCommandFactory(controller, (Command)Activator.CreateInstance(type)));
                        continue;
                    }
                }
            }

            return factories;
        }

        public static ICollection<CommandFactory> LoadFromDirectory(string directory, CommandController controller)
        {
            List<CommandFactory> factories = new List<CommandFactory>();

            foreach (string fileName in Directory.GetFiles(directory, "*.dll"))
            {
                factories.AddRange(LoadFromFile(fileName, controller));
            }

            return factories;
        }

        public static ICollection<CommandFactory> LoadFromFile(string fileName, CommandController controller)
        {
            string filePath = Path.GetFullPath(fileName);
            return LoadFromAssembly(Assembly.LoadFile(filePath), controller);
        }

        static bool InheritsFrom(Type type, Type baseType)
        {
            if (type.BaseType == null)
                return false;

            return (type.BaseType == baseType)
                || InheritsFrom(type.BaseType, baseType);
        }

        static bool IsCommandFactory(Type type)
        {
            return InheritsFrom(type, typeof(CommandFactory));
        }

        static bool IsExported(Type type)
        {
            return (type.GetCustomAttributes(typeof(ExportAttribute), false).Length > 0);
        }
    }
}
