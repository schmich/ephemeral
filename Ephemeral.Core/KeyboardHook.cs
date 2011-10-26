using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Ephemeral
{
    public delegate void KeyEventHandler(KeyHookEventArgs e);

    public interface KeyEvents
    {
        event KeyEventHandler KeyDown;
        event KeyEventHandler KeyUp;
    }

    public class KeyHook
    {
        public KeyHook(KeyEvents events, Keys key)
        {
            _events = events;
            _events.KeyDown += e => KeyDown(e);
            _events.KeyUp += e => KeyUp(e);

            Key = key;
        }

        public Keys Key
        {
            get;
            private set;
        }

        public bool SuppressInput
        {
            get;
            set;
        }

        public event KeyEventHandler KeyDown = delegate { };
        public event KeyEventHandler KeyUp = delegate { };

        KeyEvents _events;
    }

    public class KeyHookEventArgs
    {
        public Keys Key
        {
            get;
            set;
        }

        public bool Control
        {
            get;
            set;
        }

        public bool Alt
        {
            get;
            set;
        }

        public bool Shift
        {
            get;
            set;
        }

        public bool Windows
        {
            get;
            set;
        }

        public bool IsRepeat
        {
            get;
            set;
        }
    }

    public class KeyboardHook : IDisposable
    {
        public KeyboardHook()
        {
            _hookId = SetKeyboardHook();
        }

        ~KeyboardHook()
        {
            Dispose(false);
        }

        public KeyHook CreateKeyHook(Keys key)
        {
            var events = new InternalKeyEvents();
            var hook = new KeyHook(events, key);

            _hooks[key] = new KeyHookSettings
            {
                Hook = hook,
                Events = events
            };

            return hook;
        }

        public event KeyEventHandler KeyDown
        {
            add
            {
                _globalKeyDownHandlers++;
                _globalKeyDown += value;
            }

            remove
            {
                _globalKeyDownHandlers--;
                _globalKeyDown -= value;
            }
        }

        public event KeyEventHandler KeyUp
        {
            add
            {
                _globalKeyUpHandlers++;
                _globalKeyUp += value;
            }

            remove
            {
                _globalKeyUpHandlers--;
                _globalKeyUp -= value;
            }
        }

        public bool SuppressInput
        {
            get;
            set;
        }

        bool IsKeyPressed(Platform.VirtualKeyStates key)
        {
            const ushort highBit = ushort.MaxValue & ~short.MaxValue;
            return (Platform.GetKeyState(key) & highBit) != 0;
        }

        IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool isKeyDown = (wParam == (IntPtr)Platform.WM_KEYDOWN) || (wParam == (IntPtr)Platform.WM_SYSKEYDOWN);
                bool isKeyUp = (wParam == (IntPtr)Platform.WM_KEYUP) || (wParam == (IntPtr)Platform.WM_SYSKEYUP);
                bool handled = false;
                bool isRepeat = false;

                Keys key = (Keys)Marshal.ReadInt32(lParam);

                if (isKeyDown)
                {
                    if (_keysDown.Contains(key))
                    {
                        isRepeat = true;
                    }
                    else
                    {
                        _keysDown.Add(key);
                    }
                }
                else
                {
                    _keysDown.Remove(key);
                }

                if (isKeyDown || isKeyUp)
                {
                    KeyHookSettings settings;
                    if (_hooks.TryGetValue(key, out settings))
                    {
                        handled = true;

                        var args = GetKeyEventArgs(key, isRepeat);

                        if (isKeyDown)
                        {
                            settings.Events.FireKeyDown(args);
                        }
                        else
                        {
                            settings.Events.FireKeyUp(args);
                        }

                        if (settings.Hook.SuppressInput)
                        {
                            return (IntPtr)1;
                        }
                    }
                }

                if (!handled)
                {
                    bool handleGlobal =
                        (isKeyDown && (_globalKeyDownHandlers > 0)) ||
                        (isKeyUp && (_globalKeyUpHandlers > 0));

                    if (handleGlobal)
                    {
                        var eventArgs = GetKeyEventArgs(key, isRepeat);

                        if (isKeyDown)
                        {
                            _globalKeyDown(eventArgs);
                        }
                        else
                        {
                            _globalKeyUp(eventArgs);
                        }

                        if (SuppressInput)
                            return (IntPtr)1;
                    }
                }
            }

            return Platform.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        KeyHookEventArgs GetKeyEventArgs(Keys key, bool isRepeat)
        {
            return new KeyHookEventArgs
            {
                Key = key,
                Control = IsKeyPressed(Platform.VirtualKeyStates.VK_CONTROL),
                Alt = IsKeyPressed(Platform.VirtualKeyStates.VK_MENU),
                Shift = IsKeyPressed(Platform.VirtualKeyStates.VK_SHIFT),
                Windows = IsKeyPressed(Platform.VirtualKeyStates.VK_LWIN) || IsKeyPressed(Platform.VirtualKeyStates.VK_RWIN),
                IsRepeat = isRepeat
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No managed resources.
            }

            // Native resources.
            ClearKeyboardHook();
        }

        IntPtr SetKeyboardHook()
        {
            _hookCallback = new Platform.LowLevelHookProc(KeyboardHookCallback);
            using (Process thisProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule thisModule = thisProcess.MainModule)
                {
                    return Platform.SetWindowsHookEx(
                        Platform.WH_KEYBOARD_LL,
                        _hookCallback,
                        Platform.GetModuleHandle(thisModule.ModuleName),
                        0
                    );
                }
            }
        }

        void ClearKeyboardHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                Platform.UnhookWindowsHookEx(_hookId);
            }
        }

        class InternalKeyEvents : KeyEvents
        {
            public void FireKeyDown(KeyHookEventArgs e)
            {
                KeyDown(e);
            }

            public void FireKeyUp(KeyHookEventArgs e)
            {
                KeyUp(e);
            }

            public event KeyEventHandler KeyDown = delegate { };
            public event KeyEventHandler KeyUp = delegate { };
        }

        class KeyHookSettings
        {
            public KeyHook Hook;
            public InternalKeyEvents Events;
        }

        // Important to keep as a member:
        // A callback was made on a garbage collected delegate of type 'Ephemeral!Ephemeral.Platform+HookFunction::Invoke'.
        // This may cause application crashes, corruption and data loss. When passing delegates to unmanaged code,
        // they must be kept alive by the managed application until it is guaranteed that they will never be called.
        Platform.LowLevelHookProc _hookCallback;
        IntPtr _hookId;

        event KeyEventHandler _globalKeyUp = delegate { };
        event KeyEventHandler _globalKeyDown = delegate { };
        int _globalKeyDownHandlers;
        int _globalKeyUpHandlers;

        static HashSet<Keys> _keysDown = new HashSet<Keys>();
        Dictionary<Keys, KeyHookSettings> _hooks = new Dictionary<Keys, KeyHookSettings>();
    }
}
