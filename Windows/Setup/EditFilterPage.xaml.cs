using GTA5MenuTools.Extend;
using System;
using System.Collections;
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

namespace GTA5MenuTools.EditWindows
{
    /// <summary>
    /// EditFilterPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditFilterPage : Page
    {

        public EditWindowUpdateHandler? UpdateCallback;
        public string FilePath;//传入的字典路径
        /// <summary>
        /// 当前选中的模型
        /// </summary>
        private TKFilterModel? CurrModel;
        /// <summary>
        /// 当前正在编辑的模型
        /// </summary>
        private TKFilterModel? EditModel;

        /// <summary>
        /// 列表数据
        /// </summary>
        private List<TKFilterModel> FilterList;

        
        public void ConfigSubviewsEnableNo()
        {
            SaveButton.Visibility = Visibility.Collapsed;
            RemoveButton.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
            FaitourTF.IsEnabled = false;
            TargetTF.IsEnabled = false;
        }

        public EditFilterPage(string filePath)
        {
            InitializeComponent();
            
            FilePath = filePath;
            TKDictionaryModel? model = TKDataManager.Read(FilePath);
            if (model != null)
            {
                FilterList = model.FilterModes.ToList();
            }
            else
            {
                FilterList = new List<TKFilterModel>();
            }
            if (FilterList.Count > 0)//如果没有Model，内存中新建一条
            {
                CurrModel = FilterList.First();//设置当前模型为第一个
                EditModel = CurrModel.Clone();//复制当前模型用于编辑
                ReloadListView();//刷新列表框
                ReloadDetails();//刷新UI
                FilterListBox.SelectedIndex = 0;//设置列表框选中为第0位
            }
        }

        public void AddNewAnalysisModel()
        {
            TKFilterModel model;
            if (CurrModel != null)
            {
                model = CurrModel.Clone() as TKFilterModel;
            }
            else
            {
                model = new TKFilterModel();
            }
            //复制一份现在的
            int index = FilterListBox.SelectedIndex == -1 ? 0 : FilterListBox.SelectedIndex;

            FilterList.Insert(index, model);
            CurrModel = model;
            EditModel = CurrModel.Clone();
            TKDataManager.WriteFilterList(FilterList,FilePath);//最新编辑的列表保存到本地
            ReloadDetails();//刷新详情
            ReloadListView();//刷新列表
            FilterListBox.SelectedIndex = index;//设置列表选中项为当前
        }

        /// <summary>
        /// 刷新列表为最新的数据源
        /// </summary>
        public void ReloadListView()
        {
            FilterListBox.Items.Clear();
            foreach (TKFilterModel model in FilterList)
            {
                FilterListBox.Items.Add($"[{model.Target}]{model.Faitour}");
            }
        }

        /// <summary>
        /// 根据当前Model刷新页面
        /// </summary>
        public void ReloadDetails()
        {
            if (EditModel != null)
            {
                FaitourTF.Text = EditModel.Faitour;
                TargetTF.Text = EditModel.Target;
            }
            else
            {
                FaitourTF.Text = "";
                TargetTF.Text = "";
            }
            
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewAnalysisModel();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null)
            {
                return;
            }
            if (CurrModel == null)
            {
                return;
            }
            int index = FilterList.IndexOf(CurrModel);//找到Index
            FilterList.RemoveAt(index);
            TKDataManager.WriteFilterList(FilterList, FilePath);
            if (FilterList.Count > 0)
            {
                CurrModel = FilterList.First();
                EditModel = CurrModel.Clone();
            }
            else
            {
                CurrModel = null;
                EditModel = null;
            }
            
            ReloadDetails();
            ReloadListView();
            if (UpdateCallback != null)
            {
                UpdateCallback();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null)
            {
                return;
            }
            if (CurrModel == null)
            {
                return;
            }
            if (TargetTF.Text.Length == 0)
            {
                MessageBox.Show("请输入目标文本", "提示");
                return;
            }
            EditModel.Target = TargetTF.Text;
            if (FaitourTF.Text.Length == 0)
            {
                MessageBox.Show("请输入翻译文本", "提示");
                return;
            }
            EditModel.Faitour = FaitourTF.Text;
            int index = FilterList.IndexOf(CurrModel);//找到Index
            FilterList.RemoveAt(index);//从内存中移除旧Model
            FilterList.Insert(index, EditModel);//插入新编辑的Model到原来的位置
            TKDataManager.WriteFilterList(FilterList, FilePath);//最新编辑的列表保存到本地
            CurrModel = EditModel;//设置当前选中Model为新编辑的Model
            EditModel = CurrModel.Clone();//复制出来一份新的用于下次编辑
            ReloadDetails();//刷新界面显示
            ReloadListView();//刷新列表
            FilterListBox.SelectedIndex = index;
            if (UpdateCallback != null)
            {
                UpdateCallback();
            }
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = FilterListBox.SelectedIndex;
            if (index >= 0)
            {
                TKFilterModel model = FilterList[index];
                this.CurrModel = model;
                this.EditModel = model.Clone();//复制一份新的对象 用于撤回编辑操作
                this.ReloadDetails();
            }
        }
    }
}
