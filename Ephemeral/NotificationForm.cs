using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace Ephemeral
{
    public partial class NotificationForm : Form
    {
        public NotificationForm(string text)
            : this(text, TimeSpan.FromMilliseconds(1000))
        {
        }

        public NotificationForm(string text, TimeSpan minimumLifeTime)
        {
            InitializeComponent();
            _container.Font = new Font(Gentium.FontFamily, 30, FontStyle.Regular);

            Rectangle screen = Screen.GetWorkingArea(this);
            Top = (int)((screen.Height / 3f) - (Height / 2f));
            Left = (int)((screen.Width / 2f) - (Width / 2f));

            Region = Region.FromHrgn(
                Platform.CreateRoundRectRgn(0, 0, Width, Height, 20, 20)
            );

            _messageLabel.Centered = true;
            _messageLabel.Text = text;
            base.Text = text;

            _createdTime = DateTime.Now;
            _minimumLifeTime = minimumLifeTime;

            SetInputHooks();
        }

        public static void Show(string text)
        {
            NotificationForm notification = new NotificationForm(text);
            notification.Show();
        }

        public static void Show(string text, TimeSpan minimumLifeTime)
        {
            NotificationForm notification = new NotificationForm(text, minimumLifeTime);
            notification.Show();
        }

        protected override bool ShowWithoutActivation
        {
            // When the form is shown, do not give it focus.
            get { return true; }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                FadeEffect.BeginFade(this, 0, 0.85, 100);
            }

            base.OnVisibleChanged(e);
        }

        void OnInput()
        {
            if ((DateTime.Now - _createdTime) < _minimumLifeTime)
            {
                // We haven't shown the notification for the minimum amount of time yet,
                // so we'll ignore this input.
                return;
            }

            ClearInputHooks();

            FadeEffect.BeginFade(this, Opacity, 0d, 250, delegate
            {
                Invoke(new Action(delegate { Close(); }));
            });
        }

        void SetInputHooks()
        {
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyDown += e => {
                if (!e.IsRepeat)
                    OnInput();
            };

            _mouseHook = new MouseHook();
            _mouseHook.MouseMove += new MouseMoveEventHandler(delegate { OnInput(); });
            _mouseHook.MouseClick += new MouseClickEventHandler(OnInput);
        }

        void ClearInputHooks()
        {
            _keyboardHook.Dispose();
            _keyboardHook = null;

            _mouseHook.Dispose();
            _mouseHook = null;
        }

        DateTime _createdTime;
        TimeSpan _minimumLifeTime;

        KeyboardHook _keyboardHook;
        MouseHook _mouseHook;
    }
}