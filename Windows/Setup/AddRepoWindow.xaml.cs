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
using System.Xml.Linq;

namespace GTA5MenuTools
{

    public delegate void AddRepoWindowSaveHandler(TKRepoModel model);
    public delegate void AddRepoWindowRemoveHandler(TKRepoModel model);
    /// <summary>
    /// AddRepoWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddRepoWindow : Window
    {

        public TKRepoModel? RepoModel;
        public AddRepoWindowSaveHandler? SaveCallback;
        public AddRepoWindowRemoveHandler? RemoveCallback;

        public AddRepoWindow(TKRepoModel? model)
        {
            InitializeComponent();
            this.Title = "修改变量";
            RepoModel = model;
            Height = 300;
            if (model == null)
            {
                this.Title = "新建变量";
                NameTF.Text = "名称";
                TypeComboBox.SelectedIndex = 0;
                Height = 270;
                RemoveButton.Visibility = Visibility.Hidden;
            }
            ReloadData();
        }
        /// <summary>
        /// 刷新界面
        /// </summary>
        public void ReloadData()
        {
            if (RepoModel != null)
            {
                NameTF.Text = RepoModel.Name;
                TypeComboBox.SelectedIndex = (int)RepoModel.RegexType + 1;
                if (RepoModel.RegexType == TKRepoModelRegexType.Left)
                {
                    TypeComboBox.SelectedIndex = 0;
                    LeftTF.Text = "";
                    CenterTF.Text = "";
                    if (RepoModel.Loc.Length > 0)
                    {
                        RightTF.Text = RepoModel.Loc.First();
                    }
                    
                }
                else if (RepoModel.RegexType == TKRepoModelRegexType.Right)
                {
                    TypeComboBox.SelectedIndex = 1;
                    RightTF.Text = "";
                    CenterTF.Text = "";
                    if (RepoModel.Loc.Length > 0)
                    {
                        LeftTF.Text = RepoModel.Loc.First();
                    }
                    
                }
                else if (RepoModel.RegexType == TKRepoModelRegexType.Middle)
                {
                    TypeComboBox.SelectedIndex = 2;
                    if (RepoModel.Loc.Length > 0)
                    {
                        LeftTF.Text = RepoModel.Loc.First();
                    }
                    if (RepoModel.Loc.Length > 1)
                    {
                        RightTF.Text = RepoModel.Loc.Last();
                    }
                    CenterTF.Text = "";
                }
            }
        }
        /// <summary>
        /// 保存按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (RepoModel == null)
            {
                RepoModel = TKRepoModel.CreateRepo(new string[] { }, TKRepoModelRegexType.Left, "");
            }
            if (NameTF.Text.Length == 0)
            {
                MessageBox.Show("请输入变量名称", "提示");
                return;
            }

            if (TypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("请选择识别方式", "提示");
                return;
            }

            RepoModel.Name = NameTF.Text;

            if (TypeComboBox.SelectedIndex == 0)
            {
                if (RightTF.Text.Length == 0)
                {
                    MessageBox.Show("请输入中间识别节点文字","提示");
                    return;
                }
                RepoModel.RegexType = TKRepoModelRegexType.Left;
                RepoModel.Loc = new string[] { RightTF.Text };
            }
            else if (TypeComboBox.SelectedIndex == 1)
            {
                if (LeftTF.Text.Length == 0)
                {
                    MessageBox.Show("请输入中间识别节点文字", "提示");
                    return;
                }
                RepoModel.RegexType = TKRepoModelRegexType.Right;
                RepoModel.Loc = new string[] { LeftTF.Text };
            }
            else if (TypeComboBox.SelectedIndex == 2)
            {
                if (LeftTF.Text.Length == 0)
                {
                    MessageBox.Show("请输入左侧识别节点文字", "提示");
                    return;
                }
                if (RightTF.Text.Length == 0)
                {
                    MessageBox.Show("请输右侧识别节点文字", "提示");
                    return;
                }
                RepoModel.RegexType = TKRepoModelRegexType.Middle;
                RepoModel.Loc = new string[] { LeftTF.Text ,RightTF.Text};
            }

            if (SaveCallback != null)
            {
                SaveCallback(RepoModel);
            }
            this.Close();
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeComboBox.SelectedIndex == 0)
            {
                LeftTF.Visibility = Visibility.Collapsed;
                CenterTF.Visibility = Visibility.Collapsed;
                RightTF.Visibility = Visibility.Visible;
            }
            else if (TypeComboBox.SelectedIndex == 1)
            {
                LeftTF.Visibility = Visibility.Visible;
                CenterTF.Visibility = Visibility.Collapsed;
                RightTF.Visibility = Visibility.Collapsed;
            }
            else if (TypeComboBox.SelectedIndex == 2)
            {
                LeftTF.Visibility = Visibility.Visible;
                CenterTF.Visibility = Visibility.Collapsed;
                RightTF.Visibility = Visibility.Visible;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (RemoveCallback != null && RepoModel != null)
            {
                RemoveCallback(RepoModel);
            }
            this.Close();
        }
    }
}
