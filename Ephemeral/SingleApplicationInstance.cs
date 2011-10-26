using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Security.AccessControl;

namespace Ephemeral
{
    class SingleApplicationInstance
    {
        public static bool IsAlreadyRunning()
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string mutexName = string.Format("39612DC6-8C35-464F-9697-CCC5E31C6038-{0}", assembly.FullName);

            bool created = false;
            Mutex sharedMutex = new Mutex(true, mutexName, out created);

            return !created;
        }
    }
}
