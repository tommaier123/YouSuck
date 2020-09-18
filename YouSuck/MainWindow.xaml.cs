using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace YouSuck
{
    public partial class MainWindow : Window
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static Timer timer1;
        private static float seconds = 0;
        private static List<string> browsersList = new List<string> { "chrome", "firefox", "iexplore", "safari", "opera", "edge" };
        private static int lastKey = 0;


        public MainWindow()
        {
            InitializeComponent();

            _hookID = SetHook(_proc);

            timer1 = new Timer();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);
            timer1.Interval = 1000; // in miliseconds
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string answer = "";
            bool kill = t.Hours >= 1 || t.Minutes >= 30;

            if (IsYoutubeOpen(kill)) seconds++;

            if (t.Hours > 0) answer += t.Hours + " Hour" + (t.Hours != 1 ? "s" : "") + " and ";
            if (t.Minutes > 0) answer += t.Minutes + " Minute" + (t.Minutes != 1 ? "s" : "") + " and ";
            answer += t.Seconds + " Second" + (t.Seconds != 1 ? "s" : "");

            Time.Dispatcher.BeginInvoke(() =>
            {
                Time.Text = answer;
                if (kill) Time.Foreground = new SolidColorBrush(Colors.Red);
                else if (seconds <= 0) Time.Foreground = new SolidColorBrush(Colors.Green);
                else Time.Foreground = new SolidColorBrush(Colors.Black);
            });
        }

        public bool IsYoutubeOpen(bool kill = false)
        {
            bool ret = false;
            foreach (var singleBrowser in browsersList)
            {
                if (IsYoutubeInBrowser(singleBrowser, kill)) ret = true;
            }
            return ret;
        }

        public bool IsYoutubeInBrowser(string singleBrowser, bool kill = false)
        {
            bool ret = false;
            var process = Process.GetProcessesByName(singleBrowser);
            if (process.Length > 0)
            {
                foreach (Process singleProcess in process)
                {
                    IntPtr hWnd = singleProcess.MainWindowHandle;
                    int length = GetWindowTextLength(hWnd);

                    StringBuilder text = new StringBuilder(length + 1);
                    GetWindowText(hWnd, text, text.Capacity);
                    if (text.ToString().Contains("YouTube") || text.ToString().Contains("Twitch") || text.ToString().Contains("Crunchyroll") || text.ToString().Contains("Funimation") || text.ToString().Contains("Netflix"))
                    {
                        ret = true;
                        if (kill) singleProcess.CloseMainWindow();
                    }
                }
            }
            return ret;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode != lastKey)
                {
                    lastKey = vkCode;
                    seconds -= 0.3f;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            int oks = 0;
            int num = (int)MathF.Max(0, seconds) / 100 + 1;
            for (int i = 0; i < num; i++)
            {
                MessageBoxResult result = MessageBox.Show("Do you really want to close it? Are you sure?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
                if (result == MessageBoxResult.OK)
                {
                    oks++;
                }
            }
            if (oks < num)
            {
                e.Cancel = true;
            }
            else
            {
                UnhookWindowsHookEx(_hookID);
            }
        }

        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
