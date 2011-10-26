using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Ephemeral
{
    public delegate void MouseMoveEventHandler(Point position);
    public delegate void MouseClickEventHandler();

    public class MouseHook : IDisposable
    {
        public MouseHook()
        {
            _hookId = SetMouseHook();
        }

        public event MouseMoveEventHandler MouseMove = delegate { };
        public event MouseClickEventHandler MouseClick = delegate { };

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
            ClearMouseHook();
        }

        IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                switch ((int)wParam)
                {
                    case Platform.WM_MOUSEMOVE:
                        var hookInfo = (Platform.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Platform.MSLLHOOKSTRUCT));
                        MouseMove(new Point(hookInfo.pt.X, hookInfo.pt.Y));
                        break;

                    case Platform.WM_LBUTTONDOWN:
                    case Platform.WM_RBUTTONDOWN:
                    case Platform.WM_MBUTTONDOWN:
                    case Platform.WM_LBUTTONUP:
                    case Platform.WM_RBUTTONUP:
                    case Platform.WM_MBUTTONUP:
                    case Platform.WM_LBUTTONDBLCLK:
                    case Platform.WM_RBUTTONDBLCLK:
                    case Platform.WM_MBUTTONDBLCLK:
                        MouseClick();
                        break;
                }
            }

            return Platform.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        void ClearMouseHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                Platform.UnhookWindowsHookEx(_hookId);
            }
        }

        IntPtr SetMouseHook()
        {
            _hookCallback = new Platform.LowLevelHookProc(MouseHookCallback);
            using (Process thisProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule thisModule = thisProcess.MainModule)
                {
                    return Platform.SetWindowsHookEx(
                        Platform.WH_MOUSE_LL,
                        _hookCallback,
                        Platform.GetModuleHandle(thisModule.ModuleName),
                        0
                    );
                }
            }
        }

        // Important to keep as a member:
        // A callback was made on a garbage collected delegate of type 'Ephemeral!Ephemeral.Platform+HookFunction::Invoke'.
        // This may cause application crashes, corruption and data loss. When passing delegates to unmanaged code,
        // they must be kept alive by the managed application until it is guaranteed that they will never be called.
        Platform.LowLevelHookProc _hookCallback;
        IntPtr _hookId;
    }
}
