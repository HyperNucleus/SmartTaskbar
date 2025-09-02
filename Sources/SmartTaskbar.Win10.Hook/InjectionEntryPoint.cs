﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;

namespace SmartTaskbar.Hook
{
    public class InjectionEntryPoint : IEntryPoint
    {
        private readonly ServerInterface _server;

        public InjectionEntryPoint(RemoteHooking.IContext context,
                                   string                 channelName)
        {
            _server = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _server.Ping();
        }

        public void Run(RemoteHooking.IContext context,
                        string                 channelName)
        {
            var postMessageHook = LocalHook.Create(
                LocalHook.GetProcAddress("user32.dll", "PostMessageW"),
                new PostMessageDelegate(PostMessageHook),
                this);
            postMessageHook.ThreadACL.SetExclusiveACL(new[] {0});
            //RemoteHooking.WakeUpProcess();
            _server.Ping();
            try
            {
                var lastPing = Environment.TickCount;
                while (true)
                {
                    Thread.Sleep(30);

                    if (Environment.TickCount - lastPing >= 1000)
                    {
                        _server.Ping();
                        lastPing = Environment.TickCount;
                    }
                }
            }
            finally
            {
                postMessageHook?.Dispose();
                LocalHook.Release();
            }
        }

        #region PostMessage

        /// Return Type: BOOL->int
        /// hWnd: HWND->HWND__*
        /// Msg: UINT->unsigned int
        /// wParam: WPARAM->UINT_PTR->unsigned int
        /// lParam: LPARAM->LONG_PTR->int
        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool PostMessageDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static bool PostMessageHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (msg == 0x05D1
                    && lParam == IntPtr.Zero
                    && hWnd == FindWindow("Shell_TrayWnd", null))
                    return false;

                return PostMessage(hWnd, msg, wParam, lParam);
            }
            catch { return false; }
        }


        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string strClassName, string strWindowName);

        #endregion
    }
}
