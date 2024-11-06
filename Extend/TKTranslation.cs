using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace GTA5MenuTools.Extend
{
    internal class TKTranslation
    {

        public static Dictionary<string, string> HistoryDic = new Dictionary<string, string> { };

        public static string HistoryPath = Directory.GetCurrentDirectory() + "\\HISTORY.json";

        /// <summary>
        /// 从缓存读取在线翻译结果
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string? TranslationTextCache(string text)
        {
            //懒加载缓存
            if (HistoryDic.Count == 0)
            {
                ReadHistoryDic();
            }

            //缓存太多自动清理
            if (HistoryDic.Count > 200)
            {
                File.WriteAllText(HistoryPath, "");
                HistoryDic = new Dictionary<string, string>();
            }
            //判断缓存中是否存在
            if (HistoryDic.ContainsKey(text))
            {
                string result = HistoryDic[text];
                if (result != null && result.Length > 0)
                {
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 从网络获取翻译结果
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string? TranslationTextRequest(string text, string appID, string userID)
        {
            // 原文
            string q = text;
            // 源语言
            string from = "en";
            // 目标语言
            string to = "zh";
            // 改成您的APP ID
            string appId = appID;
            Random rd = new Random();
            string salt = rd.Next(100000).ToString();
            // 改成您的密钥
            string secretKey = userID;
            string sign = EncryptString(appId + q + salt + secretKey);
            string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            HttpWebRequest? request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                return null;
            }
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 2000;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            JObject? jsonObj = JObject.Parse(retString);
            string resultStr = "";
            if (jsonObj != null)
            {
                JArray? jsonArray = jsonObj["trans_result"] as JArray;
                if (jsonArray != null)
                {
                    for (int i = 0; i < jsonArray.Count; i++)
                    {
                        var item = jsonArray[i];
                        JObject? itemFirst = item as JObject;
                        if (itemFirst != null)
                        {
                            resultStr += itemFirst["dst"].ToString();
                            if (i != jsonArray.Count - 1)
                            {
                                resultStr += "\n";
                            }

                        }
                    }
                }
            }
            if (resultStr.Length > 0)
            {
                return resultStr;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 二级缓存获取翻译结果 内存 -》磁盘 -》网络
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string? TranslationText(string text, string appID, string userID)
        {
            int count = StatisticsWords(text);
            if (count > 5)
            {
                return null;
            }
            if (HistoryDic == null)
            {
                ReadHistoryDic();
            }
            string? result = TranslationTextCache(text);
            if (result != null)
            {
                return result;
            }
            else
            {
                result = TranslationTextRequest(text, appID, userID);
                if (result != null)
                {

                    HistoryDic.Add(text, result);
                    SaveHistoryDic();
                    return result;
                }
            }
            return null;
        }

        public static void SaveHistoryDic()
        {
            string json = JsonConvert.SerializeObject(HistoryDic);
            File.WriteAllText(HistoryPath, json);
        }

        public static void ReadHistoryDic()
        {
            try
            {
                //校验字典数据文件是否存在（如果不存在，释放到目录中）
                if (File.Exists(HistoryPath))
                {
                    string json = File.ReadAllText(HistoryPath);
                    Dictionary<string, string> map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    HistoryDic = map;
                }
                else
                {
                    HistoryDic = new Dictionary<string, string>();
                    SaveHistoryDic();
                }
            }
            catch (Exception)
            {
                HistoryDic = new Dictionary<string, string>();
                throw;
            }

        }

        // 计算MD5值
        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            // 将字符串转换成字节数组
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            byte[] byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }

        /// <summary>
        /// 统计字符串中英文单词数量
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public static int StatisticsWords(string words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i] > 127)
                {
                    count = count + 1;
                }
            }
            return count;
        }

    }
}
