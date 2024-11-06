// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Windows;
using System.Text;


//string[] args = Environment.GetCommandLineArgs();

if (args.Length > 1)
{
    string lPath = args[0];
    string nPath = args[1];

    Console.WriteLine("解码：low" + lPath);
    Console.WriteLine("解码：new" + nPath);

    byte[] lByte = Convert.FromBase64String(lPath);
    lPath = Encoding.Default.GetString(lByte);

    byte[] nByte = Convert.FromBase64String(nPath);
    nPath = Encoding.Default.GetString(nByte);

    Console.WriteLine("已解码：low" + lPath);
    Console.WriteLine("已解码：new" + nPath);
    Thread.Sleep(1000);
    // 移除旧文件
    if (System.IO.File.Exists(lPath))
    {
        Console.WriteLine("开始删除旧文件");
        //Thread.Sleep(2000);
        File.Delete(lPath);
        Console.WriteLine("删除旧文件完成");
        //Thread.Sleep(2000);
    }
    Thread.Sleep(1000);
    //重命名新文件为旧的文件名
    if (System.IO.File.Exists(nPath))
    {
        Console.WriteLine("开始重命名文件");
        //Thread.Sleep(2000);
        File.Move(nPath, lPath);
        Console.WriteLine("重命名文件完成");
        //Thread.Sleep(2000);
    }
    Console.WriteLine("启动应用程序：" + lPath);
    Thread.Sleep(500);
    Process p = new Process();
    p.StartInfo.FileName = lPath;
    p.Start();
    
    Console.WriteLine("正在启动...");
    Thread.Sleep(500);
    Process.GetCurrentProcess().Kill();
}
else
{
    Console.WriteLine("无效参数");
}


//file.delete(npath);
//file.move(lpath, npath);
//process p = new process();
//p.startinfo.filename = npath;
//p.start();r
//thread.sleep(1000);
//process.getcurrentprocess().kill();