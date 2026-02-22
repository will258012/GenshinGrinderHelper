using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GenshinGrinderHelper
{
    public static class WindowUtils
    {
        public readonly struct LPRECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }
        private static readonly string[] windowTitles = { "原神", "Genshin Impact" };
        private static readonly string[] classNames = { "UnityWndClass", "GenshinImpact" };
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ClientToScreen(IntPtr hlnd, ref LPRECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hwnd, StringBuilder lptrstring, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetClientRect(IntPtr hlnd, ref LPRECT rect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
        public static bool FindGameWindow(out IntPtr hWnd)
        {
            hWnd = IntPtr.Zero;

            foreach (var className in classNames)
            {
                foreach (var title in windowTitles)
                {
                    var newHWnd = FindWindow(className, title);
                    if (newHWnd != IntPtr.Zero)
                    {
                        hWnd = newHWnd;
                        return true;
                    }

                }
            }
            return false;
        }
        public static bool IsGenshinActive() => FindGameWindow(out var hWnd) && hWnd == GetForegroundWindow();
        public static void ActivateExistingWindow()
        {
            var currentProcess = Process.GetCurrentProcess();
            string processName = Process.GetCurrentProcess().ProcessName;
            var process = Process.GetProcessesByName(processName).FirstOrDefault(p => p.Id != currentProcess.Id);
            IntPtr hWnd = process.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                if (IsIconic(hWnd))
                    ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
        }
    }
}

