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

namespace GTA5MenuTools
{
    /// <summary>
    /// AddKeyWorsWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public delegate void AddKeyWorsWindowCompleteHandler(string text,string oldText);
    public delegate void AddKeyWorsWindowRemoveHandler();

    public partial class AddKeyWorsWindow : Window
    {

        public event AddKeyWorsWindowCompleteHandler? SaveCallback;//数据新增回调
        public event AddKeyWorsWindowRemoveHandler? RemoveCallback;//数据删除回调
        public string? Text;
        public string? OldText;

        public AddKeyWorsWindow(string? text)
        {
            InitializeComponent();
            Text = text;
            OldText = text;
            TextTF.Text = Text;
            if (text == null)
            {
                Height = 250;
                RemoveButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                Height = 300;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (TextTF.Text.Length > 0)
            {
                if (this.SaveCallback != null)
                {
                    this.SaveCallback(TextTF.Text, OldText == null ? "" : OldText);
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("请输入文本内容","");
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveCallback != null)
            {
                RemoveCallback();
            }
            this.Close();
        }
    }
}
