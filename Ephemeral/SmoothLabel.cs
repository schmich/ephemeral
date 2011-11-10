using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;

namespace Ephemeral
{
    public partial class SmoothLabel : Label
    {
        public SmoothLabel()
        {
            InitializeComponent();
        }

        public bool Centered
        {
            get { return _centered; }

            set
            {
                _centered = value;

                if (value)
                {
                    CenterControl(this);
                    AddCenterEvents();
                }
                else
                {
                    RemoveCenterEvents();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            base.OnPaint(e);
        }

        void AddCenterEvents()
        {
            Resize += new EventHandler(OnResize);
            TextChanged += new EventHandler(OnTextChanged);
            VisibleChanged += new EventHandler(OnVisibleChanged);
        }

        void RemoveCenterEvents()
        {
            Resize -= new EventHandler(OnResize);
            TextChanged -= new EventHandler(OnTextChanged);
            VisibleChanged -= new EventHandler(OnVisibleChanged);
        }

        void OnVisibleChanged(object sender, EventArgs e)
        {
            CenterControl(sender as Control);
        }

        void OnTextChanged(object sender, EventArgs e)
        {
            CenterControl(sender as Control);
        }

        void OnResize(object sender, EventArgs e)
        {
            CenterControl(sender as Control);
        }

        void CenterControl(Control control)
        {
            control.Left = (int)((control.Parent.Width / 2f) - (control.Width / 2f));
        }

        bool _centered;
    }
}
