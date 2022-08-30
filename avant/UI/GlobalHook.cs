
using Advent.Common.Interop;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Advent.Common.Interop;

namespace Advent.Common.UI
{
    [StructLayout(LayoutKind.Sequential)]
    internal class KeyboardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    public class GlobalHook
    {
        private static NativeMethods.HookProc mouseHookProcedure;
        private static NativeMethods.HookProc keyboardHookProcedure;
        private int keyboardHookHandle;
        private int mouseHookHandle;
        private GlobalHook.MouseHookEventHandler mouseActivity;
        private KeyEventHandler keyDown;
        private KeyPressEventHandler keyPress;
        private KeyEventHandler keyUp;

        public event GlobalHook.MouseHookEventHandler MouseActivity
        {
            add
            {
                GlobalHook.MouseHookEventHandler hookEventHandler = this.mouseActivity;
                GlobalHook.MouseHookEventHandler comparand;
                do
                {
                    comparand = hookEventHandler;
                    hookEventHandler = Interlocked.CompareExchange<GlobalHook.MouseHookEventHandler>(ref this.mouseActivity, comparand + value, comparand);
                }
                while (hookEventHandler != comparand);
            }
            remove
            {
                GlobalHook.MouseHookEventHandler hookEventHandler = this.mouseActivity;
                GlobalHook.MouseHookEventHandler comparand;
                do
                {
                    comparand = hookEventHandler;
                    hookEventHandler = Interlocked.CompareExchange<GlobalHook.MouseHookEventHandler>(ref this.mouseActivity, comparand - value, comparand);
                }
                while (hookEventHandler != comparand);
            }
        }

        public event KeyEventHandler KeyDown
        {
            add
            {
                KeyEventHandler keyEventHandler = this.keyDown;
                KeyEventHandler comparand;
                do
                {
                    comparand = keyEventHandler;
                    keyEventHandler = Interlocked.CompareExchange<KeyEventHandler>(ref this.keyDown, comparand + value, comparand);
                }
                while (keyEventHandler != comparand);
            }
            remove
            {
                KeyEventHandler keyEventHandler = this.keyDown;
                KeyEventHandler comparand;
                do
                {
                    comparand = keyEventHandler;
                    keyEventHandler = Interlocked.CompareExchange<KeyEventHandler>(ref this.keyDown, comparand - value, comparand);
                }
                while (keyEventHandler != comparand);
            }
        }

        public event KeyPressEventHandler KeyPress
        {
            add
            {
                KeyPressEventHandler pressEventHandler = this.keyPress;
                KeyPressEventHandler comparand;
                do
                {
                    comparand = pressEventHandler;
                    pressEventHandler = Interlocked.CompareExchange<KeyPressEventHandler>(ref this.keyPress, comparand + value, comparand);
                }
                while (pressEventHandler != comparand);
            }
            remove
            {
                KeyPressEventHandler pressEventHandler = this.keyPress;
                KeyPressEventHandler comparand;
                do
                {
                    comparand = pressEventHandler;
                    pressEventHandler = Interlocked.CompareExchange<KeyPressEventHandler>(ref this.keyPress, comparand - value, comparand);
                }
                while (pressEventHandler != comparand);
            }
        }

        public event KeyEventHandler KeyUp
        {
            add
            {
                KeyEventHandler keyEventHandler = this.keyUp;
                KeyEventHandler comparand;
                do
                {
                    comparand = keyEventHandler;
                    keyEventHandler = Interlocked.CompareExchange<KeyEventHandler>(ref this.keyUp, comparand + value, comparand);
                }
                while (keyEventHandler != comparand);
            }
            remove
            {
                KeyEventHandler keyEventHandler = this.keyUp;
                KeyEventHandler comparand;
                do
                {
                    comparand = keyEventHandler;
                    keyEventHandler = Interlocked.CompareExchange<KeyEventHandler>(ref this.keyUp, comparand - value, comparand);
                }
                while (keyEventHandler != comparand);
            }
        }

        public GlobalHook()
        {
        }

        public GlobalHook(bool installMouseHook, bool installKeyboardHook)
        {
            this.Start(installMouseHook, installKeyboardHook);
        }

        ~GlobalHook()
        {
            this.Stop(true, true, false);
        }

        public void Start()
        {
            this.Start(true, true);
        }

        public void Start(bool installMouseHook, bool installKeyboardHook)
        {
            if (this.mouseHookHandle == 0 && installMouseHook)
            {
                GlobalHook.mouseHookProcedure = new NativeMethods.HookProc(this.MouseHookProc);
                // In .net 4 we send in IntPtr.Zero for the hMod parameter
                this.mouseHookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, GlobalHook.mouseHookProcedure, IntPtr.Zero, 0);
                if (this.mouseHookHandle == 0)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    this.Stop(true, false, false);
                    throw new Win32Exception(lastWin32Error);
                }
            }

            if (this.keyboardHookHandle == 0 && installKeyboardHook)
            {
                GlobalHook.keyboardHookProcedure = new NativeMethods.HookProc(this.KeyboardHookProc);
                // In .net 4 we send in IntPtr.Zero for the hMod parameter
                this.keyboardHookHandle = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, GlobalHook.keyboardHookProcedure, IntPtr.Zero, 0);
                if (this.keyboardHookHandle == 0)
                {
                    int lastWin32Error2 = Marshal.GetLastWin32Error();
                    this.Stop(false, true, false);
                    throw new Win32Exception(lastWin32Error2);
                }
            }
        }

        public void Stop()
        {
            this.Stop(true, true, true);
        }

        public void Stop(bool uninstallMouseHook, bool uninstallKeyboardHook, bool throwExceptions)
        {
            if (this.mouseHookHandle != 0 && uninstallMouseHook)
            {
                int num = NativeMethods.UnhookWindowsHookEx(this.mouseHookHandle);
                this.mouseHookHandle = 0;
                if (num == 0 && throwExceptions)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastWin32Error);
                }
            }
            if (this.keyboardHookHandle != 0 && uninstallKeyboardHook)
            {
                int num2 = NativeMethods.UnhookWindowsHookEx(this.keyboardHookHandle);
                this.keyboardHookHandle = 0;
                if (num2 == 0 && throwExceptions)
                {
                    int lastWin32Error2 = Marshal.GetLastWin32Error();
                    throw new Win32Exception(lastWin32Error2);
                }
            }
        }

        private int MouseHookProc(int code, int wparam, IntPtr lparam)
        {
            MouseHookEventArgs args = (MouseHookEventArgs)null;
            if (code >= 0 && this.mouseActivity != null)
            {
                MouseLLHookStruct mouseLlHookStruct = (MouseLLHookStruct)Marshal.PtrToStructure(lparam, typeof(MouseLLHookStruct));
                MouseButtons buttons = MouseButtons.None;
                short num = (short)0;
                switch (wparam)
                {
                    case NativeMethods.WM_LBUTTONDOWN:
                        buttons = MouseButtons.Left;
                        break;
                    case NativeMethods.WM_RBUTTONDOWN:
                        buttons = MouseButtons.Right;
                        break;
                    case NativeMethods.WM_MOUSEWHEEL:
                        num = (short)(mouseLlHookStruct.mouseData >> 16 & (int)ushort.MaxValue);
                        break;
                }
                int clicks = 0;
                if (buttons != MouseButtons.None)
                    clicks = wparam == NativeMethods.WM_LBUTTONDBLCLK || wparam == NativeMethods.WM_RBUTTONDBLCLK ? 2 : 1;
                args = new MouseHookEventArgs(buttons, clicks, mouseLlHookStruct.pt.X, mouseLlHookStruct.pt.Y, (int)num);
                this.mouseActivity((object)this, args);
            }
            if (args == null || !args.Handled)
                return NativeMethods.CallNextHookEx(this.mouseHookHandle, code, wparam, lparam);
            else
                return 1;
        }

        private int KeyboardHookProc(int code, int wparam, IntPtr lparam)
        {
            bool flag1 = false;
            if (code >= 0 && (this.keyDown != null || this.keyUp != null || this.keyPress != null))
            {
                KeyboardHookStruct keyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lparam, typeof(KeyboardHookStruct));
                if (this.keyDown != null && (wparam == 256 || wparam == 260))
                {
                    KeyEventArgs e = new KeyEventArgs((Keys)keyboardHookStruct.vkCode);
                    this.keyDown((object)this, e);
                    flag1 = e.Handled;
                }
                if (this.keyPress != null && wparam == 256)
                {
                    bool flag2 = ((int)NativeMethods.GetKeyState(16) & 128) == 128;
                    bool flag3 = (int)NativeMethods.GetKeyState(20) != 0;
                    byte[] numArray = new byte[256];
                    NativeMethods.GetKeyboardState(numArray);
                    byte[] lpwTransKey = new byte[2];
                    if (NativeMethods.ToAscii(keyboardHookStruct.vkCode, keyboardHookStruct.scanCode, numArray, lpwTransKey, keyboardHookStruct.flags) == 1)
                    {
                        char ch = (char)lpwTransKey[0];
                        if (flag3 ^ flag2 && char.IsLetter(ch))
                            ch = char.ToUpper(ch);
                        KeyPressEventArgs e = new KeyPressEventArgs(ch);
                        this.keyPress((object)this, e);
                        flag1 = flag1 || e.Handled;
                    }
                }
                if (this.keyUp != null && (wparam == 257 || wparam == 261))
                {
                    KeyEventArgs e = new KeyEventArgs((Keys)keyboardHookStruct.vkCode);
                    this.keyUp((object)this, e);
                    flag1 = flag1 || e.Handled;
                }
            }
            if (!flag1)
                return NativeMethods.CallNextHookEx(this.keyboardHookHandle, code, wparam, lparam);
            else
                return 1;
        }

        public delegate void MouseHookEventHandler(object sender, MouseHookEventArgs args);
    }
}
