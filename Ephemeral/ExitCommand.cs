using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ephemeral.Commands
{
    [Export]
    public class ExitCommand : Command
    {
        public override string Name
        {
            get { return "Exit"; }
        }

        public override void Execute(string arguments)
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
