using System;
using System.Runtime.InteropServices;

namespace ProductionDocumentationSync
{
    internal static class NativeMethods
    {
        /// <summary>Örnekler arası bildirim: wParam 1 = otomatik çalışma atlandı, 2 = ikinci manuel örnek açılmak istendi.</summary>
        public const int WM_APP_NOTIFY = 0x8000 + 0x101;
        public const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }

    /// <summary>Görev çubuğu düğmesinde ilerleme (Windows 7+, ITaskbarList3). Desteklenmiyorsa sessizce atlar.</summary>
    internal static class TaskbarProgress
    {
        public enum State
        {
            NoProgress = 0,
            Indeterminate = 1,
            Normal = 2,
            Error = 4,
            Paused = 8
        }

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig] void HrInit();
            [PreserveSig] void AddTab(IntPtr hwnd);
            [PreserveSig] void DeleteTab(IntPtr hwnd);
            [PreserveSig] void ActivateTab(IntPtr hwnd);
            [PreserveSig] void SetActiveAlt(IntPtr hwnd);
            // ITaskbarList2
            [PreserveSig] void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
            // ITaskbarList3 (yalnızca kullanılanlar; sıra vtable ile aynı olmalı)
            [PreserveSig] void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            [PreserveSig] void SetProgressState(IntPtr hwnd, State state);
        }

        [ComImport]
        [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance { }

        private static ITaskbarList3 _instance;
        private static bool _failed;

        private static ITaskbarList3 Instance
        {
            get
            {
                if (_instance == null && !_failed)
                {
                    try
                    {
                        _instance = (ITaskbarList3)new TaskbarInstance();
                        _instance.HrInit();
                    }
                    catch { _failed = true; _instance = null; }
                }
                return _instance;
            }
        }

        public static void SetState(IntPtr hwnd, State state)
        {
            try { var i = Instance; if (i != null && hwnd != IntPtr.Zero) i.SetProgressState(hwnd, state); } catch { }
        }

        public static void SetValue(IntPtr hwnd, long completed, long total)
        {
            try { var i = Instance; if (i != null && hwnd != IntPtr.Zero) i.SetProgressValue(hwnd, (ulong)Math.Max(0, completed), (ulong)Math.Max(1, total)); } catch { }
        }
    }
}
