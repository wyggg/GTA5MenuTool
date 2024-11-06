using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GTA5MenuTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public EventWaitHandle? ProgramStarted { get; set; }
        private const int SW_SHOWNOMAL = 1;

        ///<summary>
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWindow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        /// <summary>
        /// 只打开一个进程
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string mutexName = "32283F61-EC4D-43B1-9C44-40280D5854DD";

            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, mutexName, out var createNew);

            if (!createNew)
            {
                try
                {
                    var processes = Process.GetProcessesByName("GTA5MenuTools");
                    if (!processes.Any())
                    {
                        MessageBox.Show("请不要重复打开", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    }
                    else
                    {
                        MessageBox.Show("请不要重复打开", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        foreach (Process process in processes)
                        {
                            ShowWindowAsync(process.MainWindowHandle, SW_SHOWNOMAL);
                            SetForegroundWindow(process.MainWindowHandle);
                            SwitchToThisWindow(process.MainWindowHandle, true);
                        }
                    }
                }
                catch (Exception)
                {
                    // Logger.Error(exception, "唤起已启动进程时出错");
                }

                App.Current.Shutdown();
                Environment.Exit(-1);
            }
            else
            {
                base.OnStartup(e);
            }
        }

    }
}
