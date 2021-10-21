﻿using System.Runtime.InteropServices;
using static SmartTaskbar.SafeNativeMethods;

namespace SmartTaskbar
{
    internal static class MaximizeWindow
    {
        private const uint SwMaximize = 3;

        private static TagWindowPlacement _placement = new()
            {length = (uint) Marshal.SizeOf(typeof(TagWindowPlacement))};


        internal static bool IsNotMaximizeWindow(this IntPtr handle)
        {
            _ = GetWindowPlacement(handle, ref _placement);
            return _placement.showCmd != SwMaximize;
        }


        internal static bool IsNotFullScreenWindow(this IntPtr handle)
        {
            _ = GetWindowRect(handle, out var tagRect);
            var monitor = Screen.FromHandle(handle);
            return tagRect.top != monitor.Bounds.Top
                   || tagRect.bottom != monitor.Bounds.Bottom
                   || tagRect.left != monitor.Bounds.Left
                   || tagRect.right != monitor.Bounds.Right;
        }
    }
}
