using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace GTA5MenuTools.Windows.EditWindows
{

    internal class TKConfigManager
    {
        private static TKConfigModel? ConfigModel;

        public static TKConfigModel GetConfigModel()
        {
            if (ConfigModel == null)
            {
                //本地不存在配置文件
                if (!System.IO.File.Exists(TKGlobalGet.kCONFIG_PATH))
                {
                    TKConfigModel model = new TKConfigModel();
                    ConfigModel = model;//存内存
                    SetConfigModel(model);//写本地
                    return ConfigModel;
                }
                else
                {
                    string json = System.IO.File.ReadAllText(TKGlobalGet.kCONFIG_PATH);
                    TKConfigModel? model = JsonConvert.DeserializeObject<TKConfigModel>(json);
                    if (model != null)
                    {
                        ConfigModel = model;//存内存
                        return ConfigModel;
                    }
                    else
                    {
                        return new TKConfigModel();
                    }
                }
            }
            else
            {
                return ConfigModel;
            }

        }

        public static void SetConfigModel(TKConfigModel model)
        {
            ConfigModel = model;
            string dicFilePath = TKGlobalGet.kCONFIG_PATH;
            string json = JsonConvert.SerializeObject(model);
            System.IO.File.WriteAllText(dicFilePath, json);
        }

    }

    internal class TKConfigModel
    {
        /// <summary>
        /// 百度翻译APPID
        /// </summary>
        public string BaiduAppID { get; set; } = "";
        /// <summary>
        /// 百度翻译用户ID
        /// </summary>
        public string BaiduUserID { get; set; } = "";

        /// <summary>
        /// 通知弹出位置
        /// </summary>
        public int ToastShowLocType { get; set; } = 5;

        /// <summary>
        /// 通知弹出时间
        /// </summary>
        public int ToastShowTime { get; set; } = 10;

        /// <summary>
        /// 通知标题字体大小
        /// </summary>
        public double ToastTitleSize { get; set; } = 14;

        /// <summary>
        /// 通知文字字体大小
        /// </summary>
        public double ToastContentSize { get; set; } = 12;

        /// <summary>
        /// 图标宽高
        /// </summary>
        public double ToastIconSize { get; set; } = 26;

        /// <summary>
        /// 通知框体圆角
        /// </summary>
        public double ToastCornerRadius { get; set; } = 5;

        /// <summary>
        /// 通知边框宽度
        /// </summary>
        public double ToastBorderWidth { get; set; } = 0;

        /// <summary>
        /// 通知框体距离屏幕边缘距离
        /// </summary>
        public double ToastEdgeSpacing { get; set; } = 45;

        /// <summary>
        /// 通知框体初始位置
        /// </summary>
        public double ToastStartSpacing { get; set; } = 15;

        /// <summary>
        /// 通知背景颜色
        /// </summary>
        public string ToastBackgroundColor { get; set; } = "#ffffff";
    }
}
/*
options.Time = 4000;//持续时间
options.Icon = ToastIcons.WarningNot;//图标样式
options.IconForeground = (Brush)new BrushConverter().ConvertFromString("#00D91A")!;//图标颜色
options.TitleFontSize = 15;//标题大小
options.FontSize = 15;//消息文字大小
options.IconSize = 26;//图标宽高
options.CornerRadius = new CornerRadius(10);//圆角
options.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#CECECE")!;//边框颜色
options.BorderThickness = new Thickness(5);//边框宽度
options.Background = (Brush)new BrushConverter().ConvertFromString("#FFFFFF")!;//背景颜色
*/