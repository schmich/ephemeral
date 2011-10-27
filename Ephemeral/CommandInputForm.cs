using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using Ephemeral.Commands;

namespace Ephemeral
{
    partial class CommandInputForm : Form
    {
        public CommandInputForm(ICommandProvider commandProvider, ICommandHistory history)
        {
            InitializeComponent();
            Width = Screen.PrimaryScreen.Bounds.Width;
            Font = new Font(Gentium.FontFamily, 30, FontStyle.Bold);

            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            
            _commandProvider = commandProvider;
            _commandHistory = history;
            _historicCommand = _commandHistory.GetEnumerator();

            _backBrush = new SolidBrush(BackColor);
            _foreBrush = new SolidBrush(Color.FromArgb(255, 10, 40));

            _dropDownForm = new DropDownForm();
            _dropDownForm.Font = Font;
            _dropDownForm.Top = Bottom;

            _mouseHook = new MouseHook();
            _mouseHook.MouseMove += new MouseMoveEventHandler(OnMouseMove);
            _mouseHook.MouseClick += new MouseClickEventHandler(Cancel);

            _inputTextBox.TextChanged += delegate { OnInputChanged(); };
            _inputTextBox.SelectionChanged += delegate { OnInputChanged(); };

            MakeForeground();
        }

        public void Cancel()
        {
            _canceled = true;
            _inputTextBox.Clear();
            _currentCommand = null;
            FadeEffect.BeginFade(this, Opacity, 0d, 100, new Action(delegate
            {
                Close();
            }));
        }

        public void AcceptCommand()
        {
            _dropDownForm.Hide();
            FadeEffect.BeginFade(this, Opacity, 0d, 100, new Action(delegate
            {
                Close();
            }));
        }

        public bool Canceled
        {
            get { return _canceled; }
        }

        public Command Command
        {
            get { return _currentCommand; }
        }

        public string Arguments
        {
            get { return GetArguments(_inputTextBox.Text); }
        }

        void OnFormLoad(object sender, EventArgs e)
        {
            MakeForeground();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Painting background is unnecessary
            // since we handle painting ourselves.
        }

        void RenderInput(Graphics g)
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.FillRectangle(_backBrush, 0, 0, Width, Height);

            string text = _inputTextBox.Text;
            int selectionStart = _inputTextBox.SelectionStart;

            int textLeft = 0;
            int caretPosition = Math.Max(4, MeasureDisplayStringWidth(g, text.Substring(0, selectionStart), Font) - 3);

            Size textSize = TextRenderer.MeasureText(g, text, Font);
            int top = (int)((Height / 2f) - (textSize.Height / 2f));

            int minHeight = TextRenderer.MeasureText(g, "|", Font).Height;
            int minTop = (int)((Height / 2f) - (minHeight / 2f));
            
            g.DrawString(text, Font, _foreBrush, textLeft, top);
            g.FillRectangle(_foreBrush, caretPosition, Math.Min(minTop, top), 3, Math.Max(minHeight, textSize.Height));
        }

        static public int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            StringFormat format = new StringFormat();
            format.FormatFlags = StringFormatFlags.MeasureTrailingSpaces;

            RectangleF rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges =  { new CharacterRange(0, text.Length) };
            Region[] regions = new Region[1];

            format.SetMeasurableCharacterRanges(ranges);
            regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int)(rect.Right + 1.0f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            RenderInput(e.Graphics);
            base.OnPaint(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                FadeEffect.BeginFade(this, 0, 0.85, 100, null);
            }

            base.OnVisibleChanged(e);
        }
        
        void OnInputChanged()
        {
            string commandText = _inputTextBox.Text.Split(' ').First();

            var suggestions = _commandProvider.GetSuggestions(commandText);

            _currentCommand = suggestions.FirstOrDefault();

            _dropDownForm.Options = suggestions;
            _inputTextBox.Focus();

            Refresh();
        }

        string GetArguments(string input)
        {
            int argumentStart = input.IndexOf(' ');
            if (argumentStart < 0)
                return null;

            return input.Substring(argumentStart + 1);
        }

        void OnFormClose(object sender, FormClosedEventArgs e)
        {
            if (_dropDownForm != null)
            {
                _dropDownForm.Close();
                _dropDownForm.Dispose();
                _dropDownForm = null;
            }

            if (_mouseHook != null)
            {
                _mouseHook.MouseMove -= new MouseMoveEventHandler(OnMouseMove);
                _mouseHook.MouseClick -= new MouseClickEventHandler(Cancel);
                _mouseHook.Dispose();
                _mouseHook = null;
            }
        }

        void OnMouseMove(Point position)
        {
            Debug.WriteLine(position.ToString());
            if (_lastMousePosition.IsEmpty)
            {
                _lastMousePosition = position;
            }
            else
            {
                double distance = Math.Sqrt(Math.Pow(position.X - _lastMousePosition.X, 2) + Math.Pow(position.Y - _lastMousePosition.Y, 2));
                if (distance > 3)
                    Cancel();

                _lastMousePosition = position;
            }
        }

        void OnFormKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Cancel();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                AcceptCommand();
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_originalText == null)
                    _originalText = _inputTextBox.Text;

                if (_historicCommand.MoveNext())
                {
                    _inputTextBox.Text = _historicCommand.Current;
                    _inputTextBox.SelectionStart = _inputTextBox.Text.Length;
                }
                else
                    _historicCommand.MovePrev();
            }
            else if (e.KeyCode == Keys.Down)
            {
                if (_historicCommand.MovePrev())
                {
                    _inputTextBox.Text = _historicCommand.Current;
                    _inputTextBox.SelectionStart = _inputTextBox.Text.Length;
                }
                else if (_originalText != null)
                {
                    _inputTextBox.Text = _originalText;
                    _inputTextBox.SelectionStart = _inputTextBox.Text.Length;
                    _originalText = null;
                }
            }
        }

        void MakeForeground()
        {
            IntPtr handle = Handle;

            Platform.SetWindowPos(
                handle,
                Platform.HWND_TOPMOST,
                0, 0, 0, 0,
                Platform.SWP_NOSIZE | Platform.SWP_NOMOVE
            );

            Platform.ShowWindow(Handle, Platform.SW_RESTORE);

            IntPtr timeout = IntPtr.Zero;
            IntPtr ignored = IntPtr.Zero;
            Platform.SystemParametersInfo(Platform.SPI_GETFOREGROUNDLOCKTIMEOUT, 0, timeout, 0);
            Platform.SystemParametersInfo(Platform.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, ignored, Platform.SPIF_SENDCHANGE);
            Platform.BringWindowToTop(handle);
            Platform.SetForegroundWindow(handle);
            Platform.SystemParametersInfo(Platform.SPI_SETFOREGROUNDLOCKTIMEOUT, 0, timeout, Platform.SPIF_SENDCHANGE);

            _inputTextBox.Focus();
        }

        bool _canceled;
        Command _currentCommand;

        Brush _backBrush;
        Brush _foreBrush;

        DropDownForm _dropDownForm;
        ICommandProvider _commandProvider;

        MouseHook _mouseHook;
        Point _lastMousePosition;

        ICommandHistory _commandHistory;
        IBidirectionalEnumerator<string> _historicCommand;
        string _originalText = null;
    }
}