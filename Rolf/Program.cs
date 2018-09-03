using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace Rolf
{
    class Program
    {
        const int SW_HIDE = 0;
        const int SW_SHOWNORMAL = 1;
        const int SW_NORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_MAXIMIZE = 3;
        const int SW_SHOWNOACTIVATE = 4;
        const int SW_SHOW = 5;
        const int SW_MINIMIZE = 6;
        const int SW_SHOWMINNOACTIVE = 7;
        const int SW_SHOWNA = 8;
        const int SW_RESTORE = 9;

        private static Dictionary<string, int> states = new Dictionary<string, int>();

        private static IntPtr winHandle = IntPtr.Zero;

        private static string action = String.Empty;
        
        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Get the text for the window pointed to by hWnd </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }

        /// <summary> Find all windows that match the given filter </summary>
        /// <param name="filter"> A delegate that returns true for windows
        ///    that should be returned and false for windows that should
        ///    not be returned </param>
        public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
        {
            IntPtr found = IntPtr.Zero;
            List<IntPtr> windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (filter(wnd, param))
                {
                    // only add the windows that pass the filter
                    windows.Add(wnd);
                }

                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowText(wnd).Contains(titleText);
            });
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: rolf.exe \"title of window\" action");
                Console.WriteLine("Available actions:");
                Console.WriteLine("SW_HIDE");
                Console.WriteLine("SW_SHOWNORMAL");
                Console.WriteLine("SW_NORMAL");
                Console.WriteLine("SW_SHOWMINIMIZED");
                Console.WriteLine("SW_SHOWMAXIMIZED");
                Console.WriteLine("SW_MAXIMIZE");
                Console.WriteLine("SW_SHOWNOACTIVATE");
                Console.WriteLine("SW_SHOW");
                Console.WriteLine("SW_MINIMIZE");
                Console.WriteLine("SW_SHOWMINNOACTIVE");
                Console.WriteLine("SW_SHOWNA");
                Console.WriteLine("SW_RESTORE");

                Console.ReadKey();

                return;
            }

            states.Add("SW_HIDE", 0);
            states.Add("SW_SHOWNORMAL", 1);
            states.Add("SW_NORMAL", 1);
            states.Add("SW_SHOWMINIMIZED", 2);
            states.Add("SW_SHOWMAXIMIZED", 3);
            states.Add("SW_MAXIMIZE", 3);
            states.Add("SW_SHOWNOACTIVATE", 4);
            states.Add("SW_SHOW", 5);
            states.Add("SW_MINIMIZE", 6);
            states.Add("SW_SHOWMINNOACTIVE", 7);
            states.Add("SW_SHOWNA", 8);
            states.Add("SW_RESTORE", 9);

            IEnumerable <IntPtr> wins = FindWindowsWithText(args[0]);
            List<IntPtr> lWins = wins.ToList();

            if (lWins.Count > 0)
            {
                foreach (IntPtr p in lWins)
                {
                    if (p == GetForegroundWindow())  // Found our own window
                    {
                        //Console.WriteLine("Found ourselves");
                        continue;
                    }

                    winHandle = p;
                }
            }

            action = args[1];

            //Console.WriteLine("Wins: " + lWins.Count.ToString());
            //Console.ReadKey();

            if (lWins.Count == 0)
            {
                Console.WriteLine("Window not found");

                Console.ReadKey();
            }
            else
            {
                ShowWindowAsync(winHandle, 0);

                ShowWindowAsync(winHandle, states[action]);

                SetForegroundWindow(winHandle);
            }
        }
    }
}
