using GTA5MenuTools;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

internal class TKGlobalGet
{
    public static double kAPP_VERSION = 1.91;
    public static string kAPP_UPDATE_INFO = @"自动更新";

    //2Take1文件路径
    public static string kFOLDER_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)!}\\PopstarDevs\\2Take1Menu";
    public static string kFILE_NAME = @"notification.log";
    public static string kFILE_PATH = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)!}\\PopstarDevs\\2Take1Menu\\notification.log";

    //2T词库路径
    public static string UserDicPath = System.IO.Directory.GetCurrentDirectory() + "\\UserDic.json";//用户词库路径
    public static string SystemDicPath = System.IO.Directory.GetCurrentDirectory() + "\\SystemDic.json";//系统词库路径

    //配置路径
    public static string kCONFIG_PATH = System.IO.Directory.GetCurrentDirectory() + "\\ConfigData.json";//配置文件位置

    //自动更新源
    public static string UploadConfigUrlForGithub = "https://github.com/wyggg/GTA5MenuToolConfig/releases/download/1.0/Update.txt";//Github源
    //public static string UploadConfigUrlForGithubTest = "https://github.com/wyggg/GTA5MenuToolConfig/releases/download/1.0/UpdateTest.txt";//Github源
    public static string UploadConfigUrlForGitee = "https://gitee.com/wuqwer/GTA5MenuTool/releases/download/1.0/Update.txt";//Gitee源

    //当前程序运行路径
    public static string? CurrentRunPath = Environment.ProcessPath;
    //运行文件夹
    public static string CurrentRunPath2 = System.IO.Directory.GetCurrentDirectory();
    //外置重启程序释放路径
    public static string UpdateExePath = System.IO.Directory.GetCurrentDirectory() + "\\LaunchTemp.exe";
    //当前文件目录释放路径
    public static string UpdateTxtPath = System.IO.Directory.GetCurrentDirectory() + "\\LaunchConfig.txt";
    //自动更新临时文件路径
    public static string DownLoadTempPath = System.IO.Directory.GetCurrentDirectory() + "\\UpdateTemp";
}