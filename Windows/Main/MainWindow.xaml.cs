
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GTA5MenuTools.EditWindows;
using Newtonsoft.Json.Linq;
using Flurl.Http;
using Newtonsoft.Json;
using GTA5MenuTools.Extend;
using System.Diagnostics;
using Downloader;
using System.ComponentModel;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using ControlzEx.Theming;
using System.Linq;
using System.Reflection.Metadata;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Core;
using GTA5MenuTools.Windows.EditWindows;
using GTA5MenuTools.Windows.Toast;

namespace GTA5MenuTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TKNotificationParser data_tool = new TKNotificationParser();//文件监听器
        private string UpdateDownloadUrl = "";//手动更新下载地址
        private string AutoUpdateDownloadUrl = "";//自动更新下载地址
        private readonly DownloadService downloader = new();//下载管理器
        private bool IsUpdateing = false;//是否正在检查更新
        private TaskbarIcon? notifyIcon;//托盘图标实例
        private Notifier? WindowNotifier;
        private Notifier? ScreenNotifier;

        public MainWindow()
        {
            InitializeComponent();
            InitToastNotifierManager();//初始化吐司管理器
            UpdateBox.Visibility = Visibility.Collapsed;//更新窗口默认隐藏
            OutputTextBox.Document = new FlowDocument();//创建文档空文档
        }

        /*----------------------------------------窗口生命周期事件----------------------------------------*/

        /// <summary>
        /// 窗口创建完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Title = $"GG2Take1通知翻译 {TKGet.kAPP_VERSION} 测试版本";
            InstallNotifyIcon();//安装系统托盘
            CheckDicUpdates();//检查更新
            StartDataProcessor();//开始监听通知文件
            ShowWindowToast("欢迎使用GG2Take1通知翻译器\nQQ群：695831873 欢迎你的加入");

        }

        /// <summary>
        /// 窗口Size被改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //OutputTextBox.InvalidateMeasure();
        }

        /*----------------------------------------文件监听器----------------------------------------*/

        /// <summary>
        /// 启动文件监听器
        /// </summary>
        private void StartDataProcessor()
        {
            data_tool.MaxLoadCount = 999;//防止卡顿 最大加载通知数量
            data_tool.FolderPath = TKGlobalGet.kFOLDER_PATH;//菜单文件夹
            data_tool.FileName = TKGlobalGet.kFILE_NAME;//通知文件名
            data_tool.DataReload += OnDataReload;//数据刷新回调
            data_tool.ErrorHandler += OnErrorHandler;//加载错误回调
            data_tool.SystemDicPath = TKGlobalGet.SystemDicPath;
            data_tool.UserDicPath = TKGlobalGet.UserDicPath;
            data_tool.Start();//开启
        }

        /// <summary>
        /// 停止文件监听器
        /// </summary>
        private void StopDataProcessor()
        {
            data_tool.Stop();//关闭
        }

        /// <summary>
        /// 数据重载回调事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="models"></param>
        private void OnDataReload(List<TKDataModel> models, bool isReload)
        {
            if (isReload == true)
            {
                ClearText();
            }
            
            bool? IsChecked = false;
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                IsChecked = OnTranslationCheckBox.IsChecked;
            });
            if (IsChecked == true)
            {
                //将翻译失败的文本请求百度翻页接口
                foreach (var item in models)
                {
                    if (item.isSucc == false)
                    {
                        string message = item.Type.Length == 0 ? "" : "[" + item.Type + "]\n";
                        message = message + item.MessageO;

                        string? message2 = "";
                        if (isReload)
                        {
                            //数据首次读取或重载的时候，翻译历史缓存中读取结果
                            message2 = TKTranslation.TranslationTextCache(message);
                        }
                        else
                        {
                            //数据新增的时候，先从缓存中读取，如果没有则请求接口
                            TKConfigModel configModel = TKConfigManager.GetConfigModel();
                            message2 = TKTranslation.TranslationText(message, configModel.BaiduAppID, configModel.BaiduUserID);//缓存读取结果
                        }

                        if (message2 != null)
                        {
                            item.Type = "机翻结果";
                            item.Message = message2;
                            item.isSucc = true;
                        }
                    }
                }
            }
            //新数据刷新到列表
            ReloadList(models, !isReload);
            if (models.Count > 998)
            {
                ShowWindowToast("过多的数据会导致加载缓慢，建议点击删除按钮清空通知历史。");
            }
        }

        /// <summary>
        /// 将读取到的数据刷新到列表中
        /// </summary>
        /// <param name="models"></param>
        /// <param name="showToast">是否弹出通知</param>
        private void ReloadList(List<TKDataModel> models, bool showToast)
        {

            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                Title = $"GG2Take1通知翻译 {TKGlobalGet.kAPP_VERSION}";
                for (int i = 0; i < models.Count; i++)
                {
                    TKDataModel item = models[i];
                    Title = $"GG2Take1通知翻译 {TKGlobalGet.kAPP_VERSION} 正在加载{Math.Round((i + 1) * 1.0 / models.Count * 100.0, 0)}% ({i + 1}/{models.Count})";
                    
                    AppendText(item);

                    //检查是否允许弹出通知
                    if (showToast == true && OnAutoToastCheckBox.IsChecked == true)
                    {
                        //翻译失败时不推送
                        if (OnAutoToastFilterCheckBox.IsChecked == true && item.isSucc == false){ continue;}
                        ShowToast(item);
                    }
                }
                Title = $"GG2Take1通知翻译 {TKGlobalGet.kAPP_VERSION}  {OutputTextBox.Document.Blocks.Count}条数据";
            });
        }

        /// <summary>
        /// 数据读取出现错误
        /// </summary>
        /// <param name="message"></param>
        private void OnErrorHandler(string message)
        {
            ShowWindowToast(message);
        }

        /*----------------------------------------输出窗口----------------------------------------*/

        /// <summary>
        /// 输出文字，写入到界面
        /// </summary>
        /// <param name="date"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <param name="colorHax"></param>
        private void AppendText(TKDataModel item)
        {
            //根据类型决定颜色
            String notColor = "#458B00";
            
            if (item.NotLevelType == TKDataModelLevelType.None){
                
                notColor = "#EBEBEB";
            }else if (item.NotLevelType == TKDataModelLevelType.Green){
                
                notColor = "#B4EEB4";
            }else if (item.NotLevelType == TKDataModelLevelType.Yellow){
                
                notColor = "#EECFA1";
            }else if (item.NotLevelType == TKDataModelLevelType.Red){
                
                notColor = "#ff6666";
            }

            System.Windows.Media.Color c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(notColor);
            var color = new SolidColorBrush(c);

            //创建段落
            Paragraph para = new Paragraph();
            para.Margin = new Thickness(0, 20, 0, 0);

            /*
            //添加类型标签控件
            var typeLabel = new Label();
            typeLabel.FontSize = 12;
            typeLabel.Content = item.Type.Length == 0 ? "通知" : item.Type;
            typeLabel.Foreground = color;
            typeLabel.Padding = new Thickness(0, 0, 0, 0);
            para.Inlines.Add(new InlineUIContainer(typeLabel));

            //添加事件标签控件
            var dateLabel = new Label();
            dateLabel.FontSize = 12;
            dateLabel.Content = item.Date;
            dateLabel.Foreground = color;
            //dateLabel.Background = color;
            dateLabel.Padding = new Thickness(10, 0, 0, 0);
            para.Inlines.Add(new InlineUIContainer(dateLabel));
            */

            //添加类型文本
            var typeRun = new Run();
            typeRun.Foreground = color;
            typeRun.FontSize = 12;
            typeRun.Text = item.Type.Length == 0 ? "通知" : item.Type;
            para.Inlines.Add(typeRun);

            //添加空行
            para.Inlines.Add("  ");

            //添加类型文本
            var dateRun = new Run();
            dateRun.Foreground = color;
            dateRun.FontSize = 12;
            dateRun.Text = item.Date.Length == 0 ? System.DateTime.Now.ToString("G") : item.Date;
            para.Inlines.Add(dateRun);

            //添加换行
            para.Inlines.Add("\n");

            //添加文本内容
            var textRun = new Run();
            textRun.Foreground = color;
            textRun.FontSize = 14;
            textRun.Text = item.Message;
            para.Inlines.Add(textRun);

            //添加事件标签控件
            para.Inlines.Add("\n");
            var button = new Button();
            button.FontSize = 12;
            button.Content = "复制";
            button.Click += delegate {
                ShowToast(item);
            };
            para.Inlines.Add(new InlineUIContainer(button));

            //插入输出窗口并刷新
            //OutputTextBox.Document.Blocks.Add(para);
            if (OutputTextBox.Document.Blocks.FirstBlock == null)
            {
                OutputTextBox.Document.Blocks.Add(para);
            }
            else
            {
                OutputTextBox.Document.Blocks.InsertBefore(OutputTextBox.Document.Blocks.FirstBlock, para);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 清屏
        /// </summary>
        private void ClearText()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate ()
            {
                OutputTextBox.Document.Blocks.Clear();
            });
        }

        /*----------------------------------------通知窗口----------------------------------------*/

        /// <summary>
        /// 初始化通知吐司管理器
        /// </summary>
        private void InitToastNotifierManager()
        {

            if (WindowNotifier != null)
            {
                WindowNotifier.Dispose();
            }
            if (ScreenNotifier != null)
            {
                ScreenNotifier.Dispose();
            }
            TKConfigModel config = TKConfigManager.GetConfigModel();

            var corner = Corner.BottomRight;
            if (config.ToastShowLocType == 0){
                corner = Corner.TopLeft;
            }else if (config.ToastShowLocType == 1){
                corner = Corner.TopLeft;
            } else if (config.ToastShowLocType == 2){
                corner = Corner.TopRight;
            }else if (config.ToastShowLocType == 3){
                corner = Corner.BottomLeft;
            }else if (config.ToastShowLocType == 4){
                corner = Corner.BottomCenter;
            }else if (config.ToastShowLocType == 5){
                corner = Corner.BottomRight;
            }

            ScreenNotifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(
                    corner: corner,
                    offsetX: config.ToastEdgeSpacing,
                    offsetY: config.ToastStartSpacing
                );
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(config.ToastShowTime),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(10));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });

            WindowNotifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);
                cfg.DisplayOptions.TopMost = false;
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(6),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });
        }

        /// <summary>
        /// 弹出通知窗口
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        private void ShowToast(TKDataModel item)
        {
            TKConfigModel config = TKConfigManager.GetConfigModel();
            string colorHex = "#B4EEB4";
            //根据类型决定通知类型
            if (item.NotLevelType == TKDataModelLevelType.None){
                colorHex = "#B4EEB4";
            }
            else if (item.NotLevelType == TKDataModelLevelType.Green){
                colorHex = "#B4EEB4";
            }
            else if (item.NotLevelType == TKDataModelLevelType.Yellow){
                colorHex = "#EECFA1";
            } else if (item.NotLevelType == TKDataModelLevelType.Red){
                colorHex = "#ff6666";
            }
            ScreenNotifier?.ShowCustomMessage($"{item.Type}", item.Message, config.ToastTitleSize,config.ToastContentSize,colorHex);
        }

        /// <summary>
        /// 弹出通知窗口
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        private void ShowWindowToast(String message , String title = "通知")
        {
            WindowNotifier?.ShowCustomMessage(title, message);
        }

        /// <summary>
        /// 弹出通知窗口测试
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        private void ShowScreenToast_Test()
        {
            TKConfigModel config = TKConfigManager.GetConfigModel();
            //string colorHex = "#000000";
            ScreenNotifier?.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", 16, 12,
                @"#000000");
            ScreenNotifier?.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", 16, 12,
                @"#008B00");
            ScreenNotifier?.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", 16, 12,
                @"#008B00");
            ScreenNotifier?.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", 16, 12,
                @"#FFB90F");
            ScreenNotifier?.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", 16, 12,
                @"#B22222");
            //ScreenNotifier.ShowCustomMessage($"标题", "内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容内容", config.ToastTitleSize, config.ToastContentSize, colorHex);
        }


        /*----------------------------------------界面按钮点击事件----------------------------------------*/

        /// <summary>
        /// 重新加载按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            
            data_tool.Update();
        }

        /// <summary>
        /// 偏好设置按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow window = new EditWindow();
            window.Show();
            window.UpdateDicCallback += delegate
            {
                data_tool.Reload();//刷新列表
            };
            window.UpdateConfigCallback += delegate
            {
                InitToastNotifierManager();//重载通知配置
                ShowWindowToast("设置已保存");
            };
        }

        /// <summary>
        /// 删除按钮点击（清理2T通知文件）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveNotButton_Click(object sender, RoutedEventArgs e)
        {
            ClearText();
            data_tool.Clear();//清理通知缓存
            ShowWindowToast("已清理历史通知");
        }

        /// <summary>
        /// 重新加载列表按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            ShowWindowToast("开始正在重新读取数据...");
            data_tool.Reload();
        }

        /// <summary>
        /// 清理列表按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearText();
            ShowWindowToast("已清理列表，不会清理本地文件，如果需要显示请点击刷新按钮");
        }

        /// <summary>
        /// 启用百度翻译按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTranslationCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TKConfigModel configModel = TKConfigManager.GetConfigModel();
            if (configModel.BaiduUserID.Length == 0 || configModel.BaiduAppID.Length == 0)
            {
                ShowWindowToast("你还没有配置百度翻译密钥,请前往设置配置密钥！\n百度翻译API是免费的，个人每月有免费的字符数。\n如果不懂怎么配置请加群看公告");
                OnTranslationCheckBox.IsChecked = false;
            }
            else
            {
                data_tool.Update();
                TKConfigModel config = TKConfigManager.GetConfigModel();
                //ShowWindowToast($"成功开启百度翻译\n当词库未命中时将调用百度翻译将通知翻译为中文\nAPPKEY：{config.BaiduUserID}\nAPPKEY：{ config.BaiduAppID}");
            }
        }

        /// <summary>
        /// 检查更新按钮点击
        /// </summary>
        bool isEnUpdateButton = true;
        private void UpdateLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEnUpdateButton == false)
            {
                return;
            }
            isEnUpdateButton = false;
            ShowWindowToast("开始检查更新...");
            CheckDicUpdates();
            var task_1 = Task.Run(async delegate
            {
                await Task.Delay(15000);
                isEnUpdateButton = true;
                return "";
            });
        }

        /// <summary>
        /// 自动更新窗口_关闭提醒按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xUpadteButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateBox.Visibility = Visibility.Collapsed;
        }


        /*----------------------------------------自动更新逻辑----------------------------------------*/
        /// <summary>
        /// 检查更新逻辑
        /// </summary>
        private async void CheckDicUpdates()
        {
            try
            {
                var json = await TKGlobalGet.UploadConfigUrlForGithub.GetStringAsync();
                UpdateLogButton.IsEnabled = true;
                if (JsonSplit.IsJson(json))
                {
                    JObject? jsonObj = JsonConvert.DeserializeObject<JObject>(json);
                    if (jsonObj == null)
                    {
                        return;
                    }
                    //词库版本
                    string dicVersion = jsonObj["DicVersion"] == null ? "0" : jsonObj["DicVersion"]!.ToString();
                    double dicVersionD = Double.Parse(dicVersion);

                    //软件版本
                    string appVersion = jsonObj["AppVersion"] == null ? "0" : jsonObj["AppVersion"]!.ToString();
                    double appVersionD = Double.Parse(appVersion);

                    //词库下载链接
                    string dicUrl = jsonObj["DicUrl"] == null ? "" : jsonObj["DicUrl"]!.ToString();

                    //手动更新链接
                    string appUrl = jsonObj["AppUrl"] == null ? "" : jsonObj["AppUrl"]!.ToString();

                    //自动更新链接（直链）
                    string autoAppUrl = jsonObj["AutoUpdateAppUrl"] == null ? "" : jsonObj["AutoUpdateAppUrl"]!.ToString();

                    //更新标题文本
                    string updateTitle = jsonObj["UpdateTitle"] == null ? "通知" : jsonObj["UpdateTitle"]!.ToString();
                    //更新内容文本
                    string updateNote = jsonObj["UpdateNote"] == null ? "" : jsonObj["UpdateNote"]!.ToString();

                    //词库更新逻辑
                    TKDictionaryModel? systemDic = TKDataManager.Read(TKGlobalGet.SystemDicPath);
                    //当前词库版本号
                    double currDicVersion = systemDic == null ? 0 : systemDic!.Version;
                    if (currDicVersion < dicVersionD)
                    {
                        if (dicUrl != null)
                        {
                            //Toast.ShowToastPool("词库", $"发现新版词库，开始更新...\n本地词库版本:{currDicVersion}\n云端词库版本:{dicVersionD}", ToastType.SecurityNot);
                            UpdateSystemDictionary(dicUrl, dicVersionD);
                        }
                    }
                    else
                    {
                        ShowWindowToast($"本地词库版本:{currDicVersion}\n云端词库版本:{dicVersionD}","词库");
                    }

                    //APP更新逻辑
                    if (appVersionD > TKGlobalGet.kAPP_VERSION)
                    {
                        UpdateBox.Visibility = Visibility.Visible;
                        UpdateDownloadUrl = appUrl;
                        AutoUpdateDownloadUrl = autoAppUrl;

                        UpdateMessageLabel.Text = updateNote;
                        UpdateTitleLabel.Text = updateTitle;

                        if (updateNote == null)
                        {
                            updateNote = "发现新版本";
                        }
                        UpdateButton.Visibility = UpdateDownloadUrl.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                        AutoUpdateButton.Visibility = AutoUpdateDownloadUrl.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                    {
                        ShowWindowToast($"当前是最新版本\n版本号：{TKGlobalGet.kAPP_VERSION}", "检查更新");
                    }
                }
                else
                {
                    ShowWindowToast($"更新源未返回正确的格式：{TKGlobalGet.kAPP_VERSION}", "检查更新");
                }
            }

            catch (FlurlHttpTimeoutException)
            {

                // 处理超时
                ShowWindowToast($"请求超时", "检查更新");
            }
            catch (FlurlHttpException ex)
            {

                // 处理错误响应
                ShowWindowToast($"{ex.StatusCode}{ex.Message}", "检查更新失败");
            }
            catch (Exception)
            {

                ShowWindowToast("检查更新失败，请手动更新。", "检查更新失败");
                throw;
            }
        }

        /// <summary>
        /// 自动更新按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoUpadteButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (AutoUpdateDownloadUrl.Length > 0)
            {
                if (IsUpdateing == true)
                {
                    StopAutoUpdateApp();
                }
                else
                {
                    AutoUpdateApp(AutoUpdateDownloadUrl);
                }
                
            }
        }


        private void OnAutoToastCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (OnAutoToastCheckBox.IsChecked == true)
            {
                ShowWindowToast("你已开启消息推送。当有新通知时会再屏幕上显示翻译文本\n需要注意的是，你的GTA5需要设置为“无边框窗口化”");
            }
            else
            {
                ShowWindowToast("你已关闭消息推送");
            }
        }

        private void OnAutoToastFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (OnAutoToastFilterCheckBox.IsChecked == true)
            {
                ShowWindowToast("你已开启失败时不通知。当新通知词库未命中并且百度翻译失败时，将不会显示通知弹框");
            }
            else
            {
                ShowWindowToast("你已关闭失败时不通知");
            }
        }

        /// <summary>
        /// 更新系统字典逻辑
        /// </summary>
        /// <param name="url"></param>
        /// <param name="version"></param>
        private async void UpdateSystemDictionary(string url,double version)
        {

            try
            {
                var json = await url.GetStringAsync();
                if (JsonSplit.IsJson(json))
                {
                    TKDictionaryModel model = TKDataManager.WriteJson(TKGlobalGet.SystemDicPath, json, version);
                    ShowWindowToast($"当前版本:{model.Version}", "词库已更新");
                    data_tool.Reload();
                }
            }
            catch (FlurlHttpTimeoutException) { }
            catch (FlurlHttpException) { }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 开始自动更新App逻辑
        /// </summary>
        /// <param name="url"></param>
        /// <param name="version"></param>
        private void AutoUpdateApp(string url)
        {
            AutoUpdateButton.Content = @"取消下载";
            IsUpdateing = true;
            //删除旧的
            File.Delete(TKGlobalGet.DownLoadTempPath);
            DownloadLabel.Text = "连接服务器...";
            if (DownloadProgressChanged != null)
            {
                downloader.DownloadProgressChanged += DownloadProgressChanged!;
            }
            if (DownloadFileCompleted != null)
            {
                downloader.DownloadFileCompleted += DownloadFileCompleted!;
            }
            
            downloader.DownloadFileTaskAsync(url, TKGlobalGet.DownLoadTempPath);
        }

        /// <summary>
        /// 停止自动更新App逻辑
        /// </summary>
        private void StopAutoUpdateApp()
        {
            AutoUpdateButton.Content = @"自动更新";
            downloader.CancelAsync();
            downloader.Clear();
            DownloadLabel.Text = "";
            IsUpdateing = false;
        }

        /// <summary>
        /// 下载进度变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                DownloadLabel.Text = $"下载中:{e.TotalBytesToReceive / 1024.0f / 1024:0.00}MB/{e.ReceivedBytesSize / 1024.0f / 1024:0.00}MB";
            });
        }

        /// <summary>
        /// 下载文件完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var CurrentRunPath = TKGlobalGet.CurrentRunPath;
            if (CurrentRunPath == null)
            {
                return;
            }
            this.Dispatcher.BeginInvoke(() =>
            {
                if (e.Error != null)
                {
                    DownloadLabel.Text = "下载失败,请手动更新";
                    IsUpdateing = false;
                    AutoUpdateButton.Content = @"自动更新";
                }
                else
                {
                    DownloadLabel.Text = $"下载成功，程序将重新启动"; 
                    //释放外置启动程序
                    File.Delete(TKGlobalGet.UpdateExePath);
                    TKExtend.ExtractFile(Resource1.FILE_UPDATE, TKGlobalGet.UpdateExePath);

                    
                    //启动外置启动程序
                    Process p = new Process();
                    p.StartInfo.FileName = TKGlobalGet.UpdateExePath;

                    byte[] rPathBytes = Encoding.Default.GetBytes(CurrentRunPath);
                    string rPath = Convert.ToBase64String(rPathBytes);
                    byte[] dPathBytes = Encoding.Default.GetBytes(TKGlobalGet.DownLoadTempPath);
                    string dPath = Convert.ToBase64String(dPathBytes);
                    p.StartInfo.Arguments = string.Format("{0} {1}", rPath, dPath);  //向启动程序传递参数
                    p.Start();
                    
                    //结束进程
                    Process.GetCurrentProcess().Kill();
                }
            });
        }
      

        /// <summary>
        /// 手动更新按钮逻辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpadteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateDownloadUrl.Length > 0)
            { 
                TKExtend.OpenUrl(UpdateDownloadUrl);
            }
            
        }

        /*----------------------------------------系统托盘图标----------------------------------------*/

        /// <summary>
        /// 安装托盘图标
        /// </summary>
        private void InstallNotifyIcon()
        {
            notifyIcon = new TaskbarIcon();
            string? path = TKGlobalGet.CurrentRunPath;
            if (path != null)
            {
                notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(path);

            }
            notifyIcon.TrayMouseDoubleClick += delegate (object sender, RoutedEventArgs e)
            {
                Show();
            };

            ContextMenu contextMenu = new ContextMenu();
            MenuItem showHideMenuItem = new MenuItem();
            showHideMenuItem.Header = "隐藏/显示";
            showHideMenuItem.Click += delegate (object sender, RoutedEventArgs e)
            {
                if (WindowState == WindowState.Minimized || Visibility == Visibility.Hidden)
                {
                    Show();
                    WindowState = WindowState.Normal;
                }
                else
                {
                    Hide();
                }
            };
            contextMenu.Items.Add(showHideMenuItem);

            MenuItem exitMenuItem = new MenuItem();
            exitMenuItem.Header = "退出";
            exitMenuItem.Click += delegate (object sender, RoutedEventArgs e)
            {
                Process.GetCurrentProcess().Kill();
            };
            contextMenu.Items.Add(exitMenuItem);
            notifyIcon.ContextMenu = contextMenu;
        }

        /// <summary>
        /// 拦截窗口关闭时的操作
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

    }
}
