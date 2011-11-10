using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Ephemeral.Commands;
using System.ComponentModel.Composition.Hosting;

namespace Ephemeral
{
    class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            if (SingleApplicationInstance.IsAlreadyRunning())
                return 1;

            Application.SetCompatibleTextRenderingDefault(true);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException, true);
            Application.EnableVisualStyles();

            using (_keyboardHook = new KeyboardHook())
            {
                _capsHook = _keyboardHook.CreateKeyHook(Keys.CapsLock);
                _capsHook.SuppressInput = true;
                _capsHook.KeyDown += new KeyEventHandler(OnCapsKeyDown);
                _capsHook.KeyUp += new KeyEventHandler(OnCapsKeyUp);

                EnsureCapsLockDisabled();

                var thisAssembly = Assembly.GetExecutingAssembly();
                var assemblyCatalog = new AssemblyCatalog(thisAssembly);
                var coreCatalog = new AssemblyCatalog(typeof(ICommand).Assembly);
                var catalog = new AggregateCatalog(assemblyCatalog, coreCatalog);
                var container = new CompositionContainer(catalog);

                // Todo: Hack: Forcing creation of command factories.
                container.GetExportedValues<ICommandFactory>();
                _commandProvider = container.GetExportedValue<ICommandProvider>();

                NotificationForm.Show("Ephemeral");

                Application.Run();
            }

            EnsureCapsLockDisabled();

            return 0;
        }

        static void OnCapsKeyDown(KeyHookEventArgs e)
        {
            if (_commandInputForm == null)
            {
                _commandInputForm = new CommandInputForm(_commandProvider, _commandHistory);
                _commandInputForm.FormClosing += new FormClosingEventHandler(OnCommandInputFormClosing);
                _commandInputForm.Show();
            }
            else
            {
                _commandInputForm.Cancel();
            }
        }

        static void OnCommandInputFormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_commandInputForm.Canceled)
                ExecuteCommand(_commandInputForm.Command, _commandInputForm.Arguments);

            _commandInputForm.FormClosing -= new FormClosingEventHandler(OnCommandInputFormClosing);
            _commandInputForm = null;
        }

        static void OnCapsKeyUp(KeyHookEventArgs e)
        {
            //if (_commandInputForm != null)
            //{
            //    _commandInputForm.Close();
            //    _commandInputForm.Dispose();
            //    _commandInputForm = null;
            //}
        }

        static void ExecuteCommand(ICommand command, string arguments)
        {
            if (command == null)
                return;

            _commandHistory.AddCommand(string.Format("{0} {1}", command.Name, arguments));

            Thread commandExecuteThread = new Thread(new ThreadStart(delegate
            {
                command.Execute(arguments);
            }));

            commandExecuteThread.IsBackground = true;
            commandExecuteThread.Start();
        }

        static void EnsureCapsLockDisabled()
        {
            const ushort highBit = ushort.MaxValue & ~short.MaxValue;
            const ushort lowBit = 1;

            ushort capsState = Platform.GetKeyState(Platform.VirtualKeyStates.VK_CAPITAL);

            bool enabled = (capsState & lowBit) != 0;
            bool keyDown = (capsState & highBit) != 0;

            // make: 0x3A, break: 0xBA
            byte key = (byte)Platform.VirtualKeyStates.VK_CAPITAL;

            if (enabled)
            {
                if (keyDown)
                {
                    Platform.keybd_event(key, 0xBA, Platform.KEYEVENTF_EXTENDEDKEY | Platform.KEYEVENTF_KEYUP, 0);
                    Platform.keybd_event(key, 0x3A, Platform.KEYEVENTF_EXTENDEDKEY, 0);
                    Platform.keybd_event(key, 0xBA, Platform.KEYEVENTF_EXTENDEDKEY | Platform.KEYEVENTF_KEYUP, 0);
                }
                else
                {
                    Platform.keybd_event(key, 0x3A, Platform.KEYEVENTF_EXTENDEDKEY, 0);
                    Platform.keybd_event(key, 0xBA, Platform.KEYEVENTF_EXTENDEDKEY | Platform.KEYEVENTF_KEYUP, 0);
                }
            }
        }

        static MemoryCommandHistory _commandHistory = new MemoryCommandHistory();

        static KeyboardHook _keyboardHook;
        static KeyHook _capsHook;

        static List<ICommand> _commands = new List<ICommand>();
        static CommandInputForm _commandInputForm;

        static ICommandProvider _commandProvider;
    }
}