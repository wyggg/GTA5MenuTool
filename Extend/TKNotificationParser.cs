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

public delegate void DataReloadEventHandler(List<TKDataModel> models,bool isReload);//����ˢ�»ص�
public delegate void DataReloadErrorHandler(string errorMessage);//ʶ��������



public class TKNotificationParser
{

    public event DataReloadEventHandler? DataReload;//������Ҫ���¼��ػص�
    public event DataReloadErrorHandler? ErrorHandler;//����ص�

    public string SystemDicPath { get; set; } = "";//֪ͨ�ļ�Ŀ¼
    public string UserDicPath { get; set; } = "";//֪ͨ�ļ�Ŀ¼

    public int MaxLoadCount { get; set; } = 200;//��������������
    public string FolderPath { get; set; } = "";//��Ŀ¼Ŀ¼
    public string FileName { get; set; } = "";//֪ͨ�ļ�Ŀ¼

    private TKDictionaryModel DictionaryModel = new TKDictionaryModel();
    private FileSystemWatcher? s_watcher;//�ļ���������ʵ��
    private long bytcount = 0;//��ǰ��ȡ�ļ���λ��

    /// <summary>
    /// �ļ�У�����������ļ��Ƿ���ã�����������ͨ���ص����ش�����Ϣ
    /// </summary>
    /// <returns></returns>
    public bool FileCheck() {

        string filePath = FolderPath + "\\" + FileName;
        //У��֪ͨ�ļ��Ƿ����
        if (!System.IO.File.Exists(filePath))
        {
            if (ErrorHandler != null)
            {
                ErrorHandler("δ�ҵ�֪ͨ��¼�ļ���" + filePath + "\n�����Ƿ��ѿ�����֪ͨ��¼���ļ�");
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// �ӱ��ض�ȡ�����ֵ�
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
    /// ��ʼ����֪ͨ�ļ�
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
    /// ֹͣ����֪ͨ�ļ�
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
    /// �ļ����»ص�
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        Update();
    }

    /// <summary>
    /// ���¼���
    /// </summary>
    public void Reload()
    {
        bytcount = 0;
        ReadDic();
        Update();
    }

    /// <summary>
    /// �������µ�����
    /// </summary>
    public void Update()
    {
        if (s_watcher == null)
        {
            FileCheck();
            return;
        }
        FileStream fstream = new FileStream(TKGlobalGet.kFILE_PATH, FileMode.OpenOrCreate);
        if (fstream.Length <= bytcount)//���ζ�ȡ�ַ����ڻ����֮ǰ��¼���ַ���֤���ļ�ɾ���ˣ���ʱ��ƫ������Ϊ0��ͷ��ȡ���ļ�
        {
            bytcount = 0;
        }
        byte[] bData = new byte[fstream.Length];//�����ļ�����
        fstream.Seek(bytcount, SeekOrigin.Begin);//���ö�ȡ��ʼλ��Ϊ�ϴε�λ��
        fstream.Read(bData, 0, bData.Length);//��ȡ�ļ�
        string result = Encoding.UTF8.GetString(bData);//��ȡ���ת��Ϊ�ַ���

        List<TKDataModel> modelArray = Translation2Take1TextToModel(result);
        if (this.DataReload != null)
            if (bytcount == 0){
                this.DataReload(modelArray,true);//��������
            } else{
                this.DataReload(modelArray,false);//��������
            }

        bytcount = fstream.Length;//�����´ε���ʼλ��Ϊ��ǰ�ļ�ĩβ
        fstream.Flush();//��������Ļ�������ʹ�����л�������ݶ�д�뵽�ļ���
        fstream.Close();//��������Ļ�������ʹ�����л�������ݶ�д�뵽�ļ���
    }

    /// <summary>
    /// ��2take1��ʽ��֪ͨ�ı�ת��Ϊ����������ģ��
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
    /// ��2Take1��ʽ��֪ͨ�ı�ת��Ϊ����ģ��
    /// </summary>
    public List<TKDataModel> Conversion2Take1TextToModel(string str)
    {
        List<string> notList = new List<string>();
        str = str.Replace("\0", "");
        string[] tempList = str.Split("\n[");
        int start = 0;

        //�����Ż���������MaxLoadCount������
        if (tempList.Length > MaxLoadCount)
        {
            start = tempList.Length - MaxLoadCount;
        }
        string? oldText = null;
        for (int i = start; i < tempList.Length; i++)
        {
            
            string not = tempList[i];
            //�ظ�֪ͨ����ʾ
            if (string.Equals(oldText, not))
            {
                continue;
            }
            not = not.TrimStart(new char[] { '\r', '\n' }).TrimEnd(new char[] { '\r', '\n' }).TrimStart(new char[] { '\r', '\n' });//��һ����������ַ�������β���з�
            if (not.Length == 0 || not.Contains("]") == false)
            {
                continue;//�������Ч�ģ��Ƴ���һ������
            }
            else
            {
                not = "[" + not;
                notList.Add(not);
            }
            oldText = not;
        }

        //�ַ�������ת��Ϊ����ģ��
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
                    //��������ģ��
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
    /// �����ض��ֵ䷭��֪ͨ����ģ��
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public TKDataModel Translation(TKDataModel model)
    {
        //�õ����ط�����������
        TKAnalysisModel? temp = null;
        foreach (TKAnalysisModel item in this.DictionaryModel.AnalysisModels)
        {
            //���Guard���ҳ��ʺϷ��뱾��֪ͨ��AnalysisModel
            if (item.Guard.getIsThrough(model.Original))
            {
                temp = item; break;
            }
        }

        //δ�ҵ����ʵĽ�����
        if (temp == null)
        {
            //model.Type = filterText(model.Type);
            //model.Message = filterText(model.Message);
            //model.Message = TKTranslation.TranslationText(model.Original);
            return model;
        }

        //�ҵ��˺��ʵĽ�����
        TKDataModel newModel = new TKDataModel();
        newModel.Type = (temp.OutputType != null ? temp.OutputType! : model.Type);
        newModel.Original = model.Original;
        newModel.Date = model.Date;
        newModel.NotLevelType = temp.NotLevelType;
        if (temp.Repos.Length <= 0)
        {
            ///����������ǿյģ�������
            //model.Type = filterText(model.Type);
            //model.Message = filterText(model.Message);
            //model.Message = TKTranslation.TranslationText(model.Original);
            return model;
        }

        string newMessage = "";
        //��ʼ��ʿ�ƥ��
        foreach (TKRepoModel item in temp.Repos)
        {
            if (item.Text.Length <= 0)
            {
                //����ƥ�� �滻�����е��ַ�
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
                    string text = match.Groups[0].Value;//ƥ�䵽���ַ���
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
        //���µ���Ϣ����������
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
    /// ����֪ͨ�ļ�
    /// </summary>
    public void Clear()
    {
        System.IO.File.WriteAllText(FolderPath + "\\" + FileName, "");
    }

}