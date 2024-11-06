using GTA5MenuTools.Extend;
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
using System.Windows.Shapes;

namespace GTA5MenuTools.EditWindows
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : Window
    {

        public EditWindowUpdateHandler? UpdateDicCallback;
        public EditWindowUpdateHandler? UpdateConfigCallback;


        public EditWindow()
        {
            InitializeComponent();

            TabItem item= new TabItem();
            {
                EditDicPage page = new EditDicPage(TKGlobalGet.UserDicPath);
                page.UpdateCallback += delegate
                {
                    if (UpdateDicCallback != null)
                    {
                        UpdateDicCallback();
                    }
                };
                InsterPage(page, "用户词库");
            }

            {
                EditFilterPage page = new EditFilterPage(TKGlobalGet.UserDicPath);
                page.UpdateCallback += delegate
                {
                    if (UpdateDicCallback != null)
                    {
                        UpdateDicCallback();
                    }
                };
                InsterPage(page, "用户过滤表");
            }

            {
                EditDicPage page = new EditDicPage(TKGlobalGet.SystemDicPath);
#if Release
                page.ConfigSubviewsEnableNo();
#endif
                page.UpdateCallback += delegate
                {
                    if (UpdateDicCallback != null)
                    {
                        UpdateDicCallback();
                    }
                };
                InsterPage(page, "系统词库");
            }

            {
                EditFilterPage page = new EditFilterPage(TKGlobalGet.SystemDicPath);
#if Release
                page.ConfigSubviewsEnableNo();
#endif
                page.UpdateCallback += delegate
                {
                    if (UpdateDicCallback != null)
                    {
                        UpdateDicCallback();
                    }
                };
                InsterPage(page, "系统过滤表");
            }

            {
                AutoToastEditPage page = new AutoToastEditPage();
                InsterPage(page, "百度翻译&样式");
                page.UpdateConfigCallback += delegate
                {
                    if (UpdateConfigCallback != null)
                    {
                        UpdateConfigCallback();
                    }
                };
            }
            {
                AboutPage page = new AboutPage();
                InsterPage(page, "关于");
            }
        }

        public void InsterPage(Page page,string name)
        {
            TabItem item = new TabItem();
            item.Header = name;
            Frame fm = new Frame();
            item.Content = fm;
            fm.Content = page;
            PageTabControl.Items.Add(item);
        }
    }

    
}
