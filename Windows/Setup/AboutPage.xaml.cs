using System;
using System.Collections.Generic;
using System.IO;
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
using static System.Net.Mime.MediaTypeNames;

namespace GTA5MenuTools.EditWindows
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();

            //开始加载图像
            BitmapImage bim1 = new BitmapImage();
            bim1.BeginInit();
            bim1.StreamSource = new MemoryStream(Resource1.weixin);
            bim1.EndInit();
            WeiXinImage.Source = bim1;
            GC.Collect(); //强制回收资源

            BitmapImage bim2 = new BitmapImage();
            bim2.BeginInit();
            bim2.StreamSource = new MemoryStream(Resource1.zhifubao);
            bim2.EndInit();
            ZhiFuBaoImage.Source = bim2;
            GC.Collect(); //强制回收资源


            BitmapImage bim3 = new BitmapImage();
            bim3.BeginInit();
            bim3.StreamSource = new MemoryStream(Resource1.goutou);
            bim3.EndInit();
            GouTouImage.Source = bim3;
            GC.Collect(); //强制回收资源
        }
    }
}
