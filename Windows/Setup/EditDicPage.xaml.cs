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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GTA5MenuTools.EditWindows
{
    public delegate void EditWindowUpdateHandler();

    /// <summary>
    /// EditDicPage.xaml 的交互逻辑
    /// </summary>
    public partial class EditDicPage : Page
    {

        public EditWindowUpdateHandler? UpdateCallback;
        public string FilePath;//传入的字典路径
        /// <summary>
        /// 当前选中的模型
        /// </summary>
        private TKAnalysisModel? CurrModel;
        /// <summary>
        /// 当前正在编辑的模型
        /// </summary>
        private TKAnalysisModel? EditModel;

        /// <summary>
        /// 列表数据
        /// </summary>
        private List<TKAnalysisModel> DicList;

        public void ConfigSubviewsEnableNo()
        {
            MoveButtons.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Collapsed;
            BottomButtons.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
            AddWordsButton.Visibility = Visibility.Collapsed;
            AddRepoButton.Visibility = Visibility.Collapsed;
            AddRepoWordsButton.Visibility = Visibility.Collapsed;
            TitleTF.IsEnabled = false;
            LevelComboBox.IsEnabled = false;
            TypeTF.IsEnabled = false;
            DicVersionLabel.Visibility = Visibility.Visible;
        }
        public EditDicPage(string filePath)
        {
            InitializeComponent();
            FilePath = filePath;

            DicVersionLabel.Visibility = Visibility.Collapsed;
            TKDictionaryModel? model = TKDataManager.Read(FilePath);
            if (model != null)
            {
                DicList = new List<TKAnalysisModel> ( model.AnalysisModels );
            }
            else
            {
                DicList = new List<TKAnalysisModel>();
            }
            DicVersionLabel.Content = $"词库版本：V{model.Version}    {DicList.Count}条数据\n\n1.系统词库自动更新的，用户不可以编辑。\n2.如果需要自定义，请前往用户词库。";
            if (DicList.Count > 0)//如果没有Model，内存中新建一条
            {
                CurrModel = DicList.First();//设置当前模型为第一个
                EditModel = CurrModel.Clone();//复制当前模型用于编辑
                ReloadListView();//刷新列表框
                ReloadDetails();//刷新UI
                DictaonaryListBox.SelectedIndex = 0;//设置列表框选中为第0位
            }
            
        }

        /// <summary>
        /// 刷新列表为最新的数据源
        /// </summary>
        public void ReloadListView()
        {
            DictaonaryListBox.Items.Clear();
            foreach (TKAnalysisModel model in DicList)
            {
                DictaonaryListBox.Items.Add($"[{model.OutputType}]{model.Title}");
            }
        }

        /// <summary>
        /// 列表某一项被点击回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DictaonaryListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = DictaonaryListBox.SelectedIndex;
            if (index >= 0)
            {
                TKAnalysisModel model = DicList[index];
                this.CurrModel = model;
                this.EditModel = model.Clone();//复制一份新的对象 用于撤回编辑操作
                this.ReloadDetails();
            }
        }

        /// <summary>
        /// 根据当前Model刷新页面
        /// </summary>
        public void ReloadDetails()
        {
            if (EditModel != null)
            {
                TitleTF.Text = EditModel.Title;
                LevelComboBox.SelectedIndex = (int)EditModel.NotLevelType;
                TypeTF.Text = EditModel.OutputType;
            }
            else
            {
                TitleTF.Text = "";
                LevelComboBox.SelectedIndex = (int)TKDataModelLevelType.None;
                TypeTF.Text = "";
            }
            ReloadKeyworBox();
            ReloadRepoBox();
        }

        /// <summary>
        /// 刷新关键字框
        /// </summary>
        public void ReloadKeyworBox()
        {
            if (CurrModel == null || EditModel == null)
            {
                AddNewAnalysisModel();
            }
            MyFlowDocument flowDic = new MyFlowDocument();
            Paragraph para = new Paragraph();
            for (int i = 0; i < EditModel.Guard.Keywords.Length; i++)
            {
                string item = EditModel.Guard.Keywords[i];
                Button button = new Button();
                button.FontSize = 12;
                button.Content = item;
                button.Margin = new Thickness(0, 0, 5, 0);
                button.Background = Brushes.Wheat;
                button.BorderBrush = Brushes.Wheat;
                button.Tag = i;
                para.Inlines.Add(button);
                int clickIndex = i;
                button.Click += delegate (object sender, RoutedEventArgs e)
                {
                    //编辑文字输入框
                    AddKeyWorsWindow window = new AddKeyWorsWindow(item);
                    window.SaveCallback += delegate (string text, string oldText)
                    {
                        EditModel.Guard.Keywords[clickIndex] = text;
                        ReloadKeyworBox();
                    };
                    window.RemoveCallback += delegate
                    {
                        List<string> list = new List<string>(EditModel.Guard.Keywords);
                        list.RemoveAt(clickIndex);
                        EditModel.Guard.Keywords = list.ToArray();
                        ReloadKeyworBox();
                    };
                    window.Show();
                };
            }
            flowDic.Blocks.Add(para);
            keyWorsTF.Document = flowDic;

        }

        /// <summary>
        /// 刷新译文部分
        /// </summary>
        public void ReloadRepoBox()
        {
            if (EditModel == null || CurrModel == null)
            {
                AddNewAnalysisModel();
            }
            MyFlowDocument flowDic = new MyFlowDocument();
            Paragraph para = new Paragraph();
            for (int i = 0; i < EditModel.Repos.Length; i++)
            {
                TKRepoModel item = EditModel.Repos[i];
                Button button = new Button();
                button.FontSize = 12;
                if (item.RegexType == TKRepoModelRegexType.Text)
                {
                    button.Content = $"{item.Text}";
                    button.Background = Brushes.White;
                    button.BorderBrush = Brushes.White;
                }
                else
                {
                    button.Content = $"{item.Name}";
                    button.Background = Brushes.Wheat;
                    button.BorderBrush = Brushes.Wheat;
                }

                //按钮点击
                button.Click += delegate (object sender, RoutedEventArgs e)
                {
                    if (item.RegexType == TKRepoModelRegexType.Text)
                    {
                        //编辑文字
                        AddKeyWorsWindow window = new AddKeyWorsWindow(item.Text);
                        window.SaveCallback += delegate (string text, string oldText)
                        {
                            item.Text = text;
                            ReloadRepoBox();
                        };
                        //删除
                        window.RemoveCallback += delegate
                        {
                            List<TKRepoModel> list = new List<TKRepoModel>(EditModel.Repos);
                            list.Remove(item);
                            EditModel.Repos = list.ToArray();
                            ReloadRepoBox();
                        };
                        window.Show();
                    }
                    else
                    {
                        //编辑变量
                        AddRepoWindow window = new AddRepoWindow(item);
                        window.Show();
                        window.SaveCallback = delegate
                        {
                            ReloadRepoBox();
                        };
                        //删除
                        window.RemoveCallback += delegate
                        {
                            List<TKRepoModel> list = new List<TKRepoModel>(EditModel.Repos);
                            list.Remove(item);
                            EditModel.Repos = list.ToArray();
                            ReloadRepoBox();
                        };
                    }
                };

                para.Inlines.Add(button);
            }
            flowDic.Blocks.Add(para);
            messageTF.Document = flowDic;
        }

        /// <summary>
        /// 添加关键字按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddKeyworsButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrModel == null || EditModel == null)
            {
                AddNewAnalysisModel();
            }
            AddKeyWorsWindow window = new AddKeyWorsWindow(null);
            window.SaveCallback += delegate (string text, string oldText)
            {
                List<string> list = new List<string>(EditModel.Guard.Keywords);
                list.Add(text);
                EditModel.Guard.Keywords = list.ToArray();
                ReloadKeyworBox();
            };
            window.Show();
        }


        /// <summary>
        /// 添加译文文本按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrModel == null || EditModel == null)
            {
                AddNewAnalysisModel();
            }
            AddKeyWorsWindow window = new AddKeyWorsWindow(null);
            window.SaveCallback += delegate (string text, string oldText)
            {
                List<TKRepoModel> list = new List<TKRepoModel>(EditModel.Repos);
                TKRepoModel model = TKRepoModel.CreateText(text);
                list.Add(model);
                EditModel.Repos = list.ToArray();
                ReloadRepoBox();
            };
            window.Show();
        }

        /// <summary>
        /// 添加变量按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddRepoButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrModel == null || EditModel == null)
            {
                AddNewAnalysisModel();
            }
            AddRepoWindow window = new AddRepoWindow(null);
            window.Show();
            window.SaveCallback += delegate (TKRepoModel model)
            {
                List<TKRepoModel> list = new List<TKRepoModel>(EditModel.Repos);
                list.Add(model);
                EditModel.Repos = list.ToArray();
                ReloadRepoBox();
            };
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrModel == null || EditModel == null)
            {
                AddNewAnalysisModel();
            }

            if (TitleTF.Text.Length == 0)
            {
                MessageBox.Show("请输入名称", "提示");
                return;
            }
            EditModel.Title = TitleTF.Text;
            if (TypeTF.Text.Length == 0)
            {
                MessageBox.Show("请输入通知标题", "提示");
                return;
            }
            EditModel.OutputType = TypeTF.Text;
            if (LevelComboBox.SelectedIndex == 0)
            {
                EditModel.NotLevelType = TKDataModelLevelType.None;
            }
            else if (LevelComboBox.SelectedIndex == 1)
            {
                EditModel.NotLevelType = TKDataModelLevelType.Green;
            }
            else if (LevelComboBox.SelectedIndex == 2)
            {
                EditModel.NotLevelType = TKDataModelLevelType.Yellow;
            }
            else if (LevelComboBox.SelectedIndex == 3)
            {
                EditModel.NotLevelType = TKDataModelLevelType.Red;
            }
            else
            {
                EditModel.NotLevelType = TKDataModelLevelType.None;
            }
            if (EditModel.Guard.Keywords.Length == 0)
            {
                MessageBox.Show("请至少添加一个识别关键字", "提示");
                return;
            }
            if (EditModel.Repos.Length == 0)
            {
                MessageBox.Show("请输入译文", "提示");
                return;
            }
            int index = DicList.IndexOf(CurrModel);//找到Index
            DicList.RemoveAt(index);//从内存中移除旧Model
            DicList.Insert(index, EditModel);//插入新编辑的Model到原来的位置
            TKDataManager.WriteAnalysisList(DicList, FilePath);//最新编辑的列表保存到本地
            CurrModel = EditModel;//设置当前选中Model为新编辑的Model
            EditModel = CurrModel.Clone();//复制出来一份新的用于下次编辑
            ReloadDetails();//刷新界面显示
            ReloadListView();//刷新列表
            DictaonaryListBox.SelectedIndex = index;
            if (UpdateCallback != null)
            {
                UpdateCallback();
            }
        }

        /// <summary>
        /// 重置按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null || CurrModel == null)
            {
                return;
            }
            EditModel = CurrModel.Clone();
            ReloadDetails();
        }
        /// <summary>
        /// 新建按钮点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewAnalysisModel();
            ReloadDetails();//刷新详情
            ReloadListView();//刷新列表
        }

        public void AddNewAnalysisModel()
        {
            string[] keywords = { };
            TKGuardModel guard = new TKGuardModel(keywords);
            guard.Type = TKGuardModelType.Strict;
            TKAnalysisModel model;
            if (CurrModel != null)
            {
                model = CurrModel.Clone() as TKAnalysisModel;//复制一份现在的
                model.Title = model.Title + "new";
                int index = DictaonaryListBox.SelectedIndex == -1 ? 0 : DictaonaryListBox.SelectedIndex;
                DicList.Insert(index, model);
                DictaonaryListBox.SelectedIndex = index;//设置列表选中项为当前
            }
            else
            {
                model = new TKAnalysisModel(new TKGuardModel( new string[] { } ));//创建空数据
                model.Title = "None";
                model.OutputType = "None";
                DicList.Insert(0, model);
                DictaonaryListBox.SelectedIndex = 0;//设置列表选中项为当前

            }
            CurrModel = model;
            EditModel = CurrModel.Clone();
            TKDataManager.WriteAnalysisList(DicList, FilePath);//最新编辑的列表保存到本地
            
        }

        /// <summary>
        /// 移除当前翻译规则
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null || CurrModel == null)
            {
                return;
            }
            int index = DicList.IndexOf(CurrModel);//找到Index
            DicList.RemoveAt(index);
            TKDataManager.WriteAnalysisList(DicList, FilePath);//最新编辑的列表保存到本地
            if (DicList.Count > 0)
            {
                CurrModel = DicList.First();
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

        /// <summary>
        /// 向上移动点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUpwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null || CurrModel == null)
            {
                return;
            }
            int index = DictaonaryListBox.SelectedIndex;
            if (index == 0)
            {
                return;
            }

            if (index == -1)
            {
                return;
            }
            DicList[index] = DicList[index - 1];
            DicList[index - 1] = CurrModel;
            ReloadListView();
            DictaonaryListBox.SelectedIndex = index - 1;
        }
        /// <summary>
        /// 向下移动点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (EditModel == null || CurrModel == null )
            {
                return;
            }
            int index = DictaonaryListBox.SelectedIndex;
            if (index >= DicList.Count - 1)
            {
                return;
            }
            if (index == -1)
            {
                return;
            }
            DicList[index] = DicList[index + 1];
            DicList[index + 1] = CurrModel;
            ReloadListView();
            DictaonaryListBox.SelectedIndex = index + 1;
        }
        /// <summary>
        /// 自动分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoClassButton_Click(object sender, RoutedEventArgs e)
        {
            //自动分类容器
            List<List<TKAnalysisModel>> classList = new List<List<TKAnalysisModel>>();
            foreach (var item in DicList)
            {
                List<TKAnalysisModel>? temp = null;
                //查找容器内是否已存在该类型的数据
                foreach (var list in classList)
                {
                    TKAnalysisModel model = list.First();
                    if (String.Equals(model.OutputType, item.OutputType))
                    {
                        temp = list;
                        list.Add(item);
                        break;
                    }
                }
                if (temp == null)
                {
                    temp = new List<TKAnalysisModel> { item };
                    classList.Add(temp);
                }
            }

            DicList.Clear();
            //将分类好的数据刷新到列表
            foreach (var list in classList)
            {
                foreach (var item in list)
                {
                    DicList.Add(item);
                }
            }
            ReloadListView();
            TKDataManager.WriteAnalysisList(DicList, FilePath);//最新编辑的列表保存到本地
        }

    }

    /// <summary>
    /// 修复富文本编辑框添加控件后控件不能点击的问题
    /// </summary>
    class MyFlowDocument : FlowDocument
    {
        protected override bool IsEnabledCore
        {
            get
            {
                return true;
            }
        }
    }
}
