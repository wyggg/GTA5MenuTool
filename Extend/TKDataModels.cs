using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using Newtonsoft.Json;

public enum TKDataModelLevelType : int
{
    None = 0,
    Green = 1,
    Yellow = 2,
    Red = 3,
}

public class TKDataModel
{
    public string Type { get; set; }
    public string Date { get; set; }
    public string Message { get; set; }
    public string MessageO { get; set; }
    public string Original { get; set; }
    public TKDataModelLevelType NotLevelType;
    public bool isSucc = false;
    public int NotLevelTypeInt { 
        get {
            return (int)NotLevelType;
        } 
        set {
            NotLevelType = (TKDataModelLevelType)value;
        } 
    }

    public TKDataModel() { 
        this.NotLevelType = TKDataModelLevelType.None;
        this.Type = "";
        this.Date = "";
        this.Message = "";
        this.Original = "";
    }
}

public class TKDictionaryModel
{
    public TKAnalysisModel[] AnalysisModels { get; set; }
    public TKFilterModel[] FilterModes { get; set; }
    public double Version { get; set; } = 0;

    public TKDictionaryModel()
    {
        AnalysisModels = new TKAnalysisModel[0];
        FilterModes = new TKFilterModel[0];
    }
}

public class TKFilterModel
{
    public string Target { get; set; }
    public string Faitour { get; set; }

    public TKFilterModel()
    {
        Target = string.Empty;
        Faitour = string.Empty;
    }

    public TKFilterModel Clone()
    {
        string json = JsonConvert.SerializeObject(this);
        TKFilterModel? model = JsonConvert.DeserializeObject<TKFilterModel>(json);
        return model!;
    }
}

public class TKAnalysisModel
{
    public string Title { get; set; }
    public TKGuardModel Guard { get; set; }
    public TKRepoModel[] Repos { get; set; }
    
    public string OutputType { get; set; }
    public bool? IsFull;
    public TKDataModelLevelType NotLevelType = TKDataModelLevelType.None;

    public  TKAnalysisModel(TKGuardModel guard)
    {
        IsFull = false;
        NotLevelType = TKDataModelLevelType.None;
        Title = "";
        Guard = guard;
        Repos= new TKRepoModel[0];
        OutputType = "";
    }

    /// <summary>
    /// 拷贝一份新对象
    /// </summary>
    /// <returns></returns>
    public TKAnalysisModel Clone()
    {
        string json = JsonConvert.SerializeObject(this);
        TKAnalysisModel? model = JsonConvert.DeserializeObject<TKAnalysisModel>(json);
        return model!;
    }
}

public enum TKGuardModelType : int
{
    Any = 0,
    Strict = 1,
}

public class TKGuardModel
{
    public TKGuardModelType Type { get; set; }
    public string[] Keywords { get; set; }

    public TKGuardModel(string[] keywords)
    {
        Type = 0;
        Keywords = keywords;
    }

    public bool getIsThrough(string text)
    {
        if (this.Type == TKGuardModelType.Any)
        {
            foreach (string item in this.Keywords)
            {
                if (text.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }
        else if (this.Type == TKGuardModelType.Strict)
        {
            foreach (string item in this.Keywords)
            {
                if (!text.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

}

public enum TKRepoModelRegexType : int
{
    Text = 0,
    Middle = 1,
    Left = 2,
    Right = 3,
}

public class TKRepoModel
{
    public string Name { get; set; }
    public string Text { get; set; }
    public string[] Loc { get; set; }
    public string? Remark { get; set; }
    public TKRepoModelRegexType RegexType { get; set; }

    public TKRepoModel(string name, string text, string[] loc, string? remark, TKRepoModelRegexType regexType)
    {
        Name = name;
        Text = text;
        Loc = loc;
        Remark = remark;
        RegexType = regexType;
    }

    public static TKRepoModel CreateRepo(string[] loc, TKRepoModelRegexType regexType, string name)
    {
        TKRepoModel model = new TKRepoModel(name,"",loc,"" ,regexType);
        return model;
    }

    public static TKRepoModel CreateText(string text)
    {
        TKRepoModel model = new TKRepoModel("", text, new string[] { }, "", TKRepoModelRegexType.Text);
        return model;
    }
}