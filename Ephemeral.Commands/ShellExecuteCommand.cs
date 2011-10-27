using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Drawing;

namespace Ephemeral.Commands
{
    [Export]
    public class ShellExecuteCommandFactory : CommandFactory
    {
        public ShellExecuteCommandFactory(ICommandController controller)
            : base(controller)
        {

            _controller = controller;

            AddDirectoryCommands(@"C:\dev\shortcuts");
            //AddDirectoryCommands(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //AddDirectoryCommands(GetStartMenuFolder());
        }

        string GetStartMenuFolder()
        {
            StringBuilder folder = new StringBuilder(260 /* MAX_PATH */);
            Platform.SHGetSpecialFolderPath(IntPtr.Zero, folder, Platform.CsidlCommonStartMenu, false);
            return folder.ToString();
        }

        void AddDirectoryCommands(string directory, bool recursive = false)
        {
            if (!Directory.Exists(directory))
                return;

            var search = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var fileName in Directory.GetFiles(directory, "*", search))
            {
                AddCommandForFile(fileName);
            }

            FileSystemWatcher watcher = new FileSystemWatcher(directory);
            watcher.IncludeSubdirectories = recursive;
            watcher.Created += new FileSystemEventHandler(OnFileCreated);
            watcher.Deleted += new FileSystemEventHandler(OnFileDeleted);
            watcher.Renamed += new RenamedEventHandler(OnFileRenamed);
            watcher.EnableRaisingEvents = true;
        }

        void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            AddCommandForFile(e.FullPath);
        }

        void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            RemoveCommandForFile(e.FullPath);
        }

        void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            RemoveCommandForFile(e.OldFullPath);
            AddCommandForFile(e.FullPath);
        }

        void AddCommandForFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                string commandName;

                FileInfo fileInfo = new FileInfo(fileName);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == 0)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(Path.GetExtension(fileName), ".lnk"))
                    {
                        commandName = Path.GetFileNameWithoutExtension(fileName);
                    }
                    else
                    {
                        commandName = Path.GetFileName(fileName);
                    }

                    var command = new ShellExecuteCommand(commandName, fileName);
                    _commands[fileName] = command;
                    _controller.AddCommand(command);
                }
            }
        }

        void RemoveCommandForFile(string fileName)
        {
            Command command;
            if (_commands.TryGetValue(fileName, out command))
            {
                _commands.Remove(fileName);
                _controller.RemoveCommand(command);
            }
        }

        ICommandController _controller;
        Dictionary<string, Command> _commands = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
    }

    class ShellExecuteCommand : Command
    {
        public ShellExecuteCommand(string commandName, string filePath)
        {
            _commandName = commandName;
            _filePath = filePath;
            _icon = Platform.GetFileIcon(filePath);
        }

        public override string Name
        {
            get { return _commandName; }
        }

        public override Bitmap Icon
        {
            get { return _icon; }
        }

        public override void Execute(string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = _filePath;
            startInfo.UseShellExecute = true;

            try
            {
                Process.Start(startInfo);
            }
            catch
            {
            }
        }

        string _commandName;
        string _filePath;

        Bitmap _icon;
    }

    static partial class Platform
    {
        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(
            IntPtr hwndOwner,
            StringBuilder folderPath,
            int folderId,
            bool create
        );

        public const int CsidlCommonStartMenu = 0x16; // Common start menu folder for all users

        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        public static Bitmap GetFileIcon(string fileName)
        {
            IntPtr hImgSmall; //the handle to the system image list
            SHFILEINFO fileInfo = new SHFILEINFO();
            hImgSmall = SHGetFileInfo(fileName, 0, ref fileInfo, (uint)Marshal.SizeOf(fileInfo), SHGFI_ICON | SHGFI_LARGEICON);
            return Icon.FromHandle(fileInfo.hIcon).ToBitmap();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
    }
}
