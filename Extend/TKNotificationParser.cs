using GTA5MenuTools;
using GTA5MenuTools.Extend;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

public delegate void DataReloadEventHandler(List<TKDataModel> models,bool isReload);//数据刷新回调
public delegate void DataReloadErrorHandler(string errorMessage);//识别发生错误



public class TKNotificationParser
{

    public event DataReloadEventHandler? DataReload;//数据需要重新加载回调
    public event DataReloadErrorHandler? ErrorHandler;//错误回调

    public string SystemDicPath { get; set; } = "";//通知文件目录
    public string UserDicPath { get; set; } = "";//通知文件目录

    public int MaxLoadCount { get; set; } = 200;//单次最大加载数量
    public string FolderPath { get; set; } = "";//根目录目录
    public string FileName { get; set; } = "";//通知文件目录

    private TKDictionaryModel DictionaryModel = new TKDictionaryModel();
    private FileSystemWatcher? s_watcher;//文件监听服务实例
    private long bytcount = 0;//当前读取文件的位标

    /// <summary>
    /// 文件校验器，返回文件是否可用，若不可用则通过回调返回错误信息
    /// </summary>
    /// <returns></returns>
    public bool FileCheck() {

        string filePath = FolderPath + "\\" + FileName;
        //校验通知文件是否存在
        if (!System.IO.File.Exists(filePath))
        {
            if (ErrorHandler != null)
            {
                ErrorHandler("未找到通知记录文件：" + filePath + "\n请检查是否已开启，通知记录到文件");
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// 从本地读取翻译字典
    /// </summary>
    public void ReadDic()
    {
        List<TKAnalysisModel> aList = new List<TKAnalysisModel> { };
        List<TKFilterModel> fList = new List<TKFilterModel> { };

        TKDictionaryModel? systemDic = TKDataManager.Read(SystemDicPath);
        if (systemDic == null)
        {
            systemDic = TKDataManager.WriteDefine(SystemDicPath);
        }
        foreach (var item in systemDic.AnalysisModels)
        {
            aList.Add(item);
        }
        foreach (var item in systemDic.FilterModes)
        {
            fList.Add(item);
        }

        TKDictionaryModel? userDic = TKDataManager.Read(UserDicPath);
        if (userDic == null)
        {
            userDic = TKDataManager.WriteNone(UserDicPath);
        }
        foreach (var item in userDic.AnalysisModels)
        {
            aList.Add(item);
        }
        foreach (var item in userDic.FilterModes)
        {
            fList.Add(item);
        }
        DictionaryModel.FilterModes = fList.ToArray();
        DictionaryModel.AnalysisModels = aList.ToArray();
    }

    /// <summary>
    /// 开始监听通知文件
    /// </summary>
    public void Start()
    {
        if (s_watcher == null)
        {
            if (FileCheck())
            {
                this.ReadDic();
                bytcount = 0;
                s_watcher = new FileSystemWatcher(FolderPath, FileName);
                s_watcher.IncludeSubdirectories = true;
                s_watcher.Created += this.OnChanged;
                s_watcher.Changed += this.OnChanged;
                s_watcher.Deleted += this.OnChanged;
                s_watcher.EnableRaisingEvents = true;
                this.Update();
            }
            
        }
    }

    /// <summary>
    /// 停止监听通知文件
    /// </summary>
    public void Stop()
    {
        if (s_watcher != null)
        {
            s_watcher.Created -= this.OnChanged;
            s_watcher.Changed -= this.OnChanged;
            s_watcher.Deleted -= this.OnChanged;
            s_watcher.Dispose();
            s_watcher = null;
        }
    }

    /// <summary>
    /// 文件更新回调
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Update();
    }

    /// <summary>
    /// 重新加载
    /// </summary>
    public void Reload()
    {
        bytcount = 0;
        ReadDic();
        Update();
    }

    /// <summary>
    /// 更新最新的数据
    /// </summary>
    public void Update()
    {
        if (s_watcher == null)
        {
            FileCheck();
            return;
        }
        FileStream fstream = new FileStream(TKGlobalGet.kFILE_PATH, FileMode.OpenOrCreate);
        if (fstream.Length <= bytcount)//本次读取字符少于或等于之前记录的字符，证明文件删改了，此时将偏移量设为0重头读取该文件
        {
            bytcount = 0;
        }
        byte[] bData = new byte[fstream.Length];//创建文件容器
        fstream.Seek(bytcount, SeekOrigin.Begin);//设置读取起始位置为上次的位置
        fstream.Read(bData, 0, bData.Length);//读取文件
        string result = Encoding.UTF8.GetString(bData);//读取结果转换为字符串

        List<TKDataModel> modelArray = Translation2Take1TextToModel(result);
        if (this.DataReload != null)
            if (bytcount == 0){
                this.DataReload(modelArray,true);//数据重载
            } else{
                this.DataReload(modelArray,false);//数据新增
            }

        bytcount = fstream.Length;//设置下次的起始位置为当前文件末尾
        fstream.Flush();//清除此流的缓冲区，使得所有缓冲的数据都写入到文件中
        fstream.Close();//清除此流的缓冲区，使得所有缓冲的数据都写入到文件中
    }

    /// <summary>
    /// 将2take1格式的通知文本转换为翻译后的数据模型
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public List<TKDataModel> Translation2Take1TextToModel(string text)
    {
        List<TKDataModel> modelArray = this.Conversion2Take1TextToModel(text);
        for (int i = 0; i < modelArray.Count; i++)
        {
            modelArray[i] = Translation(modelArray[i]);
        }
        return modelArray;
    }

    /// <summary>
    /// 将2Take1格式的通知文本转换为数据模型
    /// </summary>
    public List<TKDataModel> Conversion2Take1TextToModel(string str)
    {
        List<string> notList = new List<string>();
        str = str.Replace("\0", "");
        string[] tempList = str.Split("\n[");
        int start = 0;

        //性能优化，最多加载MaxLoadCount条数据
        if (tempList.Length > MaxLoadCount)
        {
            start = tempList.Length - MaxLoadCount;
        }
        string? oldText = null;
        for (int i = start; i < tempList.Length; i++)
        {
            
            string not = tempList[i];
            //重复通知不显示
            if (string.Equals(oldText, not))
            {
                continue;
            }
            not = not.TrimStart(new char[] { '\r', '\n' }).TrimEnd(new char[] { '\r', '\n' }).TrimStart(new char[] { '\r', '\n' });//进一步处理，清除字符串的首尾换行符
            if (not.Length == 0 || not.Contains("]") == false)
            {
                continue;//如果是无效的，移除这一条数据
            }
            else
            {
                not = "[" + not;
                notList.Add(not);
            }
            oldText = not;
        }

        //字符串数组转换为数据模型
        List<TKDataModel> modelArray = new List<TKDataModel>(notList.Count);
        for (int i = 0; i < notList.Count; i++)
        {
            object? obj = notList[i];
            if (obj != null && obj is string)
            {
                string? not = obj.ToString();
                if (not != null)
                {
                    string text = not;
                    List<string> items = new List<string> { };
                    string cur = "";
                    foreach (char item in text)
                    {
                        string tempChat = item.ToString();
                        if (string.Equals(tempChat, "["))
                        {
                            cur = "";
                        }
                        else if (string.Equals(tempChat, "]"))
                        {
                            if (cur.Length > 0)
                            {
                                items.Add(cur);
                            }
                            cur = "";
                        }
                        else
                        {
                            cur = cur + item;
                        }
                    }
                    string date = "";
                    if (items.Count > 0)
                    {
                        date = items[0];
                    }

                    string type = "";
                    if (items.Count > 1)
                    {
                        type = items[1];
                    }

                    string messageO = cur;
                    string message = cur.TrimStart().TrimEnd().Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
                    //创建数据模型
                    TKDataModel model = new TKDataModel();
                    model.Date = date;
                    model.Type = type;
                    model.Message = message;
                    model.MessageO = messageO;
                    model.Original = text;
                    modelArray.Add(model);
                }
            }
        }

        return modelArray;
    }

    /// <summary>
    /// 根据特定字典翻译通知数据模型
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public TKDataModel Translation(TKDataModel model)
    {
        //得到本地翻译配置数据
        TKAnalysisModel? temp = null;
        foreach (TKAnalysisModel item in this.DictionaryModel.AnalysisModels)
        {
            //配对Guard，找出适合翻译本条通知的AnalysisModel
            if (item.Guard.getIsThrough(model.Original))
            {
                temp = item; break;
            }
        }

        //未找到合适的解析器
        if (temp == null)
        {
            //model.Type = filterText(model.Type);
            //model.Message = filterText(model.Message);
            //model.Message = TKTranslation.TranslationText(model.Original);
            return model;
        }

        //找到了合适的解析器
        TKDataModel newModel = new TKDataModel();
        newModel.Type = (temp.OutputType != null ? temp.OutputType! : model.Type);
        newModel.Original = model.Original;
        newModel.Date = model.Date;
        newModel.NotLevelType = temp.NotLevelType;
        if (temp.Repos.Length <= 0)
        {
            ///结果集定义是空的，坏数据
            //model.Type = filterText(model.Type);
            //model.Message = filterText(model.Message);
            //model.Message = TKTranslation.TranslationText(model.Original);
            return model;
        }

        string newMessage = "";
        //开始与词库匹配
        foreach (TKRepoModel item in temp.Repos)
        {
            if (item.Text.Length <= 0)
            {
                //正则匹配 替换规则中的字符
                string regexString = "";
                if (item.RegexType == TKRepoModelRegexType.Middle && item.Loc.Length > 1)
                {
                    regexString = $"(?<=({item.Loc[0]})).*?(?=({item.Loc[1]}))";
                }
                else if (item.RegexType == TKRepoModelRegexType.Left && item.Loc.Length > 0)
                {
                    regexString = $".*(?=({item.Loc[0]}))";
                }
                else if (item.RegexType == TKRepoModelRegexType.Right && item.Loc.Length > 0)
                {
                    regexString = $"(?<=({item.Loc[0]})).*";
                }
                if (regexString.Length != 0)
                {
                    Regex regex = new Regex(regexString, RegexOptions.IgnoreCase);
                    Match match = regex.Match(model.Message);
                    string text = match.Groups[0].Value;//匹配到的字符串
                    if (text.Length == 0)
                    {
                        text = "?";
                    }
                    newMessage = newMessage + text;
                }
            }
            else
            {
                newMessage = newMessage + item.Text;
            }
        }

        newModel.Type = filterText(newModel.Type);
        newModel.Message = filterText(newMessage);
        newModel.isSucc = true;
        return newModel;
    }

    public string filterText(string message)
    {
        //对新的消息过滤器处理
        foreach (var item in DictionaryModel.FilterModes)
        {
            if (message.Contains(item.Target))
            {
                message = Regex.Replace(message, item.Target, item.Faitour, RegexOptions.IgnoreCase);
            }   
        }
        return message;
    }

    /// <summary>
    /// 清理通知文件
    /// </summary>
    public void Clear()
    {
        System.IO.File.WriteAllText(FolderPath + "\\" + FileName, "");
    }

}