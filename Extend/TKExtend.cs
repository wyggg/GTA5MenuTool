using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace GTA5MenuTools.Extend
{
    internal class TKExtend
    {
        /// <summary>
        /// 判断程序是否运行
        /// </summary>
        /// <param name="appName">程序名称</param>
        /// <returns>正在运行返回true，未运行返回false</returns>
        public static bool IsAppRun(string appName)
        {
            return Process.GetProcessesByName(appName).Length > 0;
        }

        /// <summary>
        /// 打开指定路径或链接（带异常提示）
        /// </summary>
        /// <param name="path">本地文件夹路径</param>
        public static void OpenPath(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception)
            {

            }
        }


        /// <summary>
        /// 打开指定路径或链接（带异常提示）
        /// </summary>
        /// <param name="path">本地文件夹路径</param>
        public static void OpenUrl(string url)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;    //不使用shell启动
            p.StartInfo.RedirectStandardInput = true;//喊cmd接受标准输入
            p.StartInfo.RedirectStandardOutput = false;//不想听cmd讲话所以不要他输出
            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
            p.StartInfo.CreateNoWindow = true;//不显示窗口
            p.Start();

            //向cmd窗口发送输入信息 后面的&exit告诉cmd运行好之后就退出
            p.StandardInput.WriteLine("start " + url + "&exit");
            p.StandardInput.AutoFlush = true;
            p.WaitForExit();//等待程序执行完退出进程
            p.Close();
        }


        /// <summary>
        /// 文件重命名
        /// </summary>
        public static void FileReName(string OldPath, string NewPath)
        {
            var ReName = new FileInfo(OldPath);
            ReName.MoveTo(NewPath);
        }

        /// <summary>
        /// 释放内嵌资源至指定位置
        /// </summary>
        /// <param name="resource">嵌入的资源，此参数写作：命名空间.文件夹名.文件名.扩展名</param>
        /// <param name="path">释放到位置</param>
        public static void ExtractFile(byte[] buffer, string path)
        {
            FileStream file = new FileStream(path, FileMode.Create);
            file.Write(buffer, 0, buffer.Length);
            file.Flush();
            file.Close();
        }

        public static Icon ImageSourceToIcon(ImageSource imageSource)
        {
            BitmapSource bitmapSource = imageSource as BitmapSource;

            Bitmap bmp = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            IntPtr hIcon = bmp.GetHicon();
            Icon icon = (Icon)Icon.FromHandle(hIcon).Clone();

            return icon;
        }
    }
}
