using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GTA5MenuTools.EditWindows;
using GTA5MenuTools.Windows.EditWindows;

namespace GTA5MenuTools.EditWindows
{
    /// <summary>
    /// AutoToastEditPage.xaml 的交互逻辑
    /// </summary>
    public partial class AutoToastEditPage : Page
    {
        private TKConfigModel ConfigModel;
        public EditWindowUpdateHandler? UpdateConfigCallback;
        public AutoToastEditPage()
        {
            InitializeComponent();
            ConfigModel = TKConfigManager.GetConfigModel();
            this.DataContext = ConfigModel;

            Dictionary<int, string> LocItems = new Dictionary<int, string>();
            LocItems.Add(0, "左侧向下弹出");
            LocItems.Add(2, "右侧向下弹出");
            LocItems.Add(3, "左侧向上弹出");
            LocItems.Add(4, "中间向上弹出");
            LocItems.Add(5, "右侧向上弹出");
            LocComboBox.ItemsSource = LocItems;

            Dictionary<int, string> TimeItems = new Dictionary<int, string>();
            TimeItems.Add(1, "1秒");
            TimeItems.Add(2, "2秒");
            TimeItems.Add(3, "3秒");
            TimeItems.Add(4, "4秒");
            TimeItems.Add(5, "5秒");
            TimeItems.Add(6, "6秒");
            TimeItems.Add(7, "7秒");
            TimeItems.Add(8, "8秒");
            TimeItems.Add(9, "9秒");
            TimeItems.Add(10, "10秒（与2T同步）");
            TimeItems.Add(9999999, "一万年");
            TimeComboBox.ItemsSource = TimeItems;

            Dictionary<double, string> TitleFontSizeItems = new Dictionary<double, string>();
            TitleFontSizeItems.Add(12, "小号 12");
            TitleFontSizeItems.Add(14, "中号 14");
            TitleFontSizeItems.Add(16, "中号 16");
            TitleFontSizeItems.Add(22, "大号 22");
            TitleFontSizeItems.Add(26, "超大号 26");
            TitleFontSizeItems.Add(35, "特大号 35");
            TitleFontComboBox.ItemsSource = TitleFontSizeItems;

            Dictionary<double, string> ContentFontSizeItems = new Dictionary<double, string>();
            ContentFontSizeItems.Add(12, "小号 12");
            ContentFontSizeItems.Add(14, "中号 14");
            ContentFontSizeItems.Add(16, "中号 16");
            ContentFontSizeItems.Add(22, "大号 22");
            ContentFontSizeItems.Add(26, "超大号 26");
            ContentFontSizeItems.Add(35, "特大号 35");
            ContentFontComboBox.ItemsSource = ContentFontSizeItems;

            Dictionary<double, string> EdgeSpacingItems = new Dictionary<double, string>();
            EdgeSpacingItems.Add(0, "紧贴屏幕");
            EdgeSpacingItems.Add(15, "15像素的间隙");
            EdgeSpacingItems.Add(30, "30像素的间距");
            EdgeSpacingItems.Add(45, "45像素的间隙");
            EdgeSpacingItems.Add(60, "60像素的间距");
            EdgeSpacingItems.Add(75, "75像素的间距");
            EdgeSpacingItems.Add(90, "90像素的间距");
            EdgeSpacingItems.Add(100, "100像素的间距");
            EdgeSpacingItems.Add(150, "150像素的间距");
            EdgeSpacingComboBox.ItemsSource = EdgeSpacingItems;

            Dictionary<double, string> StartSpacingItems = new Dictionary<double, string>();
            StartSpacingItems.Add(0, "紧贴屏幕");
            StartSpacingItems.Add(15, "15像素的间隙");
            StartSpacingItems.Add(30, "30像素的间距");
            StartSpacingItems.Add(45, "45像素的间隙");
            StartSpacingItems.Add(60, "60像素的间距");
            StartSpacingItems.Add(75, "75像素的间距");
            StartSpacingItems.Add(90, "90像素的间距");
            StartSpacingItems.Add(100, "100像素的间距");
            StartSpacingItems.Add(150, "150像素的间距");
            StartSpacingComboBox.ItemsSource = StartSpacingItems;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TKConfigManager.SetConfigModel(ConfigModel);
            if (UpdateConfigCallback != null)
            {
                UpdateConfigCallback();
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //Toast.ShowToastPool("正常", "通知通知通知通知通知通知", ToastType.Information);
            //Toast.ShowToastPool("安全", "通知通知通知通知通知通知", ToastType.SecurityNot);
            //Toast.ShowToastPool("警告", "通知通知通知通知通知通知", ToastType.WarningNot);
            //Toast.ShowToastPool("危险", "通知通知通知通知通知通知", ToastType.DangerousNot);
        }
    }
}
