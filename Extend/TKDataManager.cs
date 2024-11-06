using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTA5MenuTools.Extend
{
    internal class TKDataManager
    {

        /// <summary>
        /// 读取字典文件
        /// </summary>
        public static TKDictionaryModel? Read(string filePath)
        {
            //校验字典数据文件是否存在（如果不存在，释放到目录中）
            if (System.IO.File.Exists(filePath))
            {
                //本地已存在字典，从文件读取字典
                string json = System.IO.File.ReadAllText(filePath);
                TKDictionaryModel? model = JsonConvert.DeserializeObject<TKDictionaryModel>(json);
                if (model != null)
                {
                    return model;
                }
            }
            return null;
        }

        /// <summary>
        /// 向本地写入翻译字典
        /// </summary>
        /// <param name="dicFilePath"></param>
        /// <param name="dictionary"></param>
        public static void Write(TKDictionaryModel model, string filePath)
        {
            string json = JsonConvert.SerializeObject(model);
            System.IO.File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 向本地写入翻译字典列表
        /// </summary>
        /// <param name="dicFilePath"></param>
        /// <param name="dictionary"></param>
        public static bool WriteFilterList(List<TKFilterModel> list, string filePath)
        {
            TKDictionaryModel? model = Read(filePath);
            if (model != null)
            {
                model.FilterModes = list.ToArray();
                Write(model, filePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向本地写入过滤器字典列表
        /// </summary>
        /// <param name="dicFilePath"></param>
        /// <param name="dictionary"></param>
        public static void WriteAnalysisList(List<TKAnalysisModel> list, string filePath)
        {
            TKDictionaryModel? model = Read(filePath);
            if (model == null)
            {
                model = WriteNone(filePath);
            }
            model.AnalysisModels = list.ToArray();
            Write(model, filePath);
        }

        /// <summary>
        /// 向一个路径覆盖写入内置词库并返回
        /// </summary>
        /// <param name="filePath"></param>
        public static TKDictionaryModel WriteDefine(string filePath)
        {
            byte[] jsonByte = Resource1.SystemDic;
            string json = Encoding.UTF8.GetString(jsonByte);
            TKDictionaryModel? model = JsonConvert.DeserializeObject<TKDictionaryModel>(json);
            if (model != null)
            {
                Write(model, filePath);
                return model;
            }
            return new TKDictionaryModel();
        }

        /// <summary>
        /// 向一个路径覆盖写入指定JSON词库并返回（必须保证Json格式时正确的，否者将引发不可预期的错误）
        /// </summary>
        /// <param name="filePath"></param>
        public static TKDictionaryModel WriteJson(string filePath, string json, double version)
        {
            TKDictionaryModel? model = JsonConvert.DeserializeObject<TKDictionaryModel>(json);
            if (model != null)
            {
                model.Version = version;
                Write(model, filePath);
                return model;
            }
            return new TKDictionaryModel();
        }

        /// <summary>
        /// 向一个路径写入空词库文件
        /// </summary>
        /// <param name="filePath"></param>
        public static TKDictionaryModel WriteNone(string filePath)
        {
            TKDictionaryModel model = new TKDictionaryModel();
            Write(model, filePath);
            return model;
        }
    }
}
