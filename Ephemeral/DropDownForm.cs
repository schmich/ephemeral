using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Ephemeral.Commands;

namespace Ephemeral
{
    public partial class DropDownForm : Form
    {
        public DropDownForm()
        {
            InitializeComponent();

            Platform.SetWindowPos(
                Handle,
                Platform.HWND_TOPMOST,
                0, 0, 0, 0,
                Platform.SWP_NOSIZE | Platform.SWP_NOMOVE
            );

            DoubleBuffered = true;
            ControlStyles styles =
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint;

            SetStyle(styles, true);

            _options = new ICommand[0];
        }

        public IEnumerable<ICommand> Options
        {
            get { return _options; }
            set
            {
                _options = value;

                if (_options.Count() > 0)
                {
                    if (!Visible)
                    {
                        Show();
                    }

                    Refresh();
                }
                else
                {
                    if (Visible)
                    {
                        Hide();
                    }
                }
            }
        }

        void RenderOptions(Graphics g, out int width, out int height)
        {
            Padding padding = new Padding(5, 5, 0, 7);
            int spacing = 5;

            width = 0;
            height = 0;

            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            foreach (ICommand option in _options)
            {
                int iconWidth = 0;
                if (option.Icon != null)
                    iconWidth = 32;

                Size size = TextRenderer.MeasureText(option.Name, Font);
                width = Math.Max(width, size.Width + iconWidth + 3);
                height += size.Height + spacing;

                if ((height + Top) > Screen.PrimaryScreen.Bounds.Height)
                    break;
            }

            width += padding.Right;
            height += padding.Bottom;

            //Color startColor = Color.FromArgb(255, 0, 79);
            //Color endColor = Color.FromArgb(183, 0, 58);
            Color startColor = Color.Black;
            Color endColor = Color.Black;

            Brush gradient = new LinearGradientBrush(
                new Point(1, 1),
                new Point(width, height),
                startColor,
                endColor
            );

            g.FillRectangle(gradient, 0, 0, width, height);

            int positionY = padding.Top;
            foreach (ICommand option in _options)
            {
                int iconSpace = 0;
                if (option.Icon != null)
                {
                    iconSpace = 32;
                    g.DrawImage(option.Icon, padding.Left, positionY + 5, 32, 32); 
                }

                g.DrawString(
                    option.Name,
                    Font,
                    Brushes.White,
                    padding.Left + iconSpace + 3,
                    positionY
                );

                positionY += TextRenderer.MeasureText(option.Name, Font).Height + spacing;

                if ((positionY + Top) > Screen.PrimaryScreen.Bounds.Height)
                    break;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Painting background is unnecessary
            // since we handle painting ourselves.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int width = 0;
            int height = 0;

            RenderOptions(e.Graphics, out width, out height);

            Width = width;
            Height = height;

            // Optimization: cache regions for different numbers of options
            IntPtr roundedRegion = Platform.CreateRoundRectRgn(0, 0, width, height, 15, 15);
            IntPtr leftSquaredRegion = Platform.CreateRectRgn(0, 0, 15, height - 1);
            IntPtr finalRegion = Platform.CreateRectRgn(0, 0, width - 1, height - 15);
            Platform.CombineRgn(finalRegion, finalRegion, leftSquaredRegion, Platform.CombineRgnStyles.RGN_OR);
            Platform.CombineRgn(finalRegion, finalRegion, roundedRegion, Platform.CombineRgnStyles.RGN_OR);

            Region = Region.FromHrgn(finalRegion);

            Platform.DeleteObject(roundedRegion);
            Platform.DeleteObject(leftSquaredRegion);
            Platform.DeleteObject(finalRegion);
            
            base.OnPaint(e);
        }

        IEnumerable<ICommand> _options;
    }
}
