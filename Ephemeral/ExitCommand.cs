using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using System.Drawing;

namespace Ephemeral.Commands
{
    [Export(typeof(ICommand))]
    public class ExitCommand : ICommand
    {
        public string Name
        {
            get { return "Exit"; }
        }

        public Bitmap Icon
        {
            get { return null; }
        }

        public void Execute(string arguments)
        {
            NotificationForm notification = new NotificationForm("Goodbye.");
            notification.FormClosed += new FormClosedEventHandler(delegate
            {
                Application.Exit();
            });

            notification.ShowDialog();
        }
    }
}
