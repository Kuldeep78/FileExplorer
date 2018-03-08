using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using FileExplorer.Annotations;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SizeViewModel myModel = new SizeViewModel();
        public MainWindow()
        {
            InitializeComponent();
            SizeGrid.DataContext = myModel;
        }
        public string SelectedImagePath { get; set; }
        string dummyNode = "Root";
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                foldersItem.Items.Add(item);
            }
        }

        private void foldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView tree = (TreeView)sender;
            TreeViewItem temp = ((TreeViewItem)tree.SelectedItem);

            if (temp == null)
                return;
            SelectedImagePath = "";
            string temp1 = "";
            string temp2 = "";
            while (true)
            {
                temp1 = temp.Header.ToString();
                if (temp1.Contains(@"\"))
                {
                    temp2 = "";
                }
                SelectedImagePath = temp1 + temp2 + SelectedImagePath;
                if (temp.Parent.GetType().Equals(typeof(TreeView)))
                {
                    break;
                }
                temp = ((TreeViewItem)temp.Parent);
                temp2 = @"\";
            }
            //show user selected path
            //MessageBox.Show(SelectedImagePath);
        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories(item.Tag.ToString()))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = s;
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
               // txtfolderinfo.Text = "test";
                myModel.SizeText = "Working....";
                ThreadPool.QueueUserWorkItem(ExecuteStep);

            }
            catch (Exception ex)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine("Error in Running prerequisites:");
                errorMessage.AppendLine("Message - " + ex.Message);
                errorMessage.AppendLine("StackTrace - " + ex.StackTrace);
                MessageBox.Show(errorMessage.ToString(), "Error");
            }

            
        }

        private void ExecuteStep(object state)
        {
            string[] dirsStrings;
            try
            {
                dirsStrings = Directory.GetDirectories(SelectedImagePath);
            }
            catch (Exception ex)
            {
                myModel.SizeText = "Error in getting folder " + ex.Message;
                return;
            }
            decimal totalsize = 0;
            List<FolderInfo> folderInfoList = new List<FolderInfo>();
            foreach (var s in dirsStrings)
            {
                try
                {
                    var size = GetDirectorySize(s);
                    var sizeString = myModel.SizeText + s + " " + size.ToString();
                    var folderInfo = new FolderInfo();
                    folderInfo.FolderName = s;
                    folderInfo.FolderSize = size;
                    totalsize += size;
                    folderInfoList.Add(folderInfo);
                    //myModel.SizeText += folderInfo.ToString() + "\n";
                }
                catch (Exception)
                {
                }
            }
            var sortedList = folderInfoList.OrderByDescending(f => f.FolderSize).ToList();
            //folderInfoList.Sort(delegate (FolderInfo x, FolderInfo y)
            //{
            //    return x.FolderSize.CompareTo(y.FolderSize);
            //}); ;
            myModel.SizeText = "";
            sortedList.ForEach(f =>
            {
                myModel.SizeText += f.ToString() + "\n";
            });
            if (string.IsNullOrEmpty(myModel.SizeText))
            {
                myModel.SizeText = " Folder is empty";
            }
            
            var totalSize = decimal.Round(Convert.ToDecimal(((totalsize / 1024) / 1024) / 1024), 2);
            myModel.TotalSizeText = "Total Size - " + totalSize + " GB";
        }


        static long GetDirectorySize(string p)
        {
            // 1.
            // Get array of all file names.
            string[] a = Directory.GetFiles(p, "*.*",SearchOption.AllDirectories);
            

            // 2.
            // Calculate total bytes of all files in a loop.
            long b = 0;
            foreach (string name in a)
            {
                // 3.
                // Use FileInfo to get length of each file.
                FileInfo info = new FileInfo(name);
                b += info.Length;
            }
            // 4.
            // Return total size
            return b;
        }
    }

    internal class FolderInfo
    {
        public string FolderName { get; internal set; }
        public float FolderSize { get; set; }

        public override string ToString()
        {
            var totalSize = decimal.Round(Convert.ToDecimal(((FolderSize/1024)/1024)/1024) ,2);
            return totalSize + " GB" + "   " + FolderName;
        }
    }

    public class SizeViewModel : INotifyPropertyChanged
    {
        private string _sizeText;
        private string _totalsizeText;

        public string SizeText
        {
            get { return _sizeText; }
            set
            {
                _sizeText = value;
                OnPropertyChanged();
            }
        }

        public string TotalSizeText
        {
            get { return _totalsizeText; }
            set
            {
                _totalsizeText = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
