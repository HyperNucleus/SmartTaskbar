using System;
using System.Collections.Generic;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace SmartTaskbar
{
    internal sealed class Engine
    {
        private static Timer _timer;
        private static TaskbarInfo _taskbar;
        private static readonly HashSet<IntPtr> DesktopHandleSet = new();
        private static readonly HashSet<IntPtr> NonDesktopShowHandleSet = new();

        public Engine(Container container)
        {
            _timer = new Timer(container)
            {
                Interval = 125
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private static void Timer_Tick(object? sender, EventArgs e)
        {
            _taskbar = TaskbarHelper.InitTaskbar();
            if (_taskbar.Handle == IntPtr.Zero)
                return;

            if (_taskbar.CheckIfDesktopShow(DesktopHandleSet, NonDesktopShowHandleSet))
            {
                _taskbar.ShowTaskar();
            }
        }
    }
}
