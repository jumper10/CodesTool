using Microsoft.Win32;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CodesTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string SelectdDir { get; set; }
        public string SelectdFile { get; set; }
        public string SelectedTag { get; set; }
        public int Count { get; set; } = 4000;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            cbbList.SelectedIndex = 0;
            tbCount.Text = Count.ToString();
        }

        private void btnDir_Click(object sender, RoutedEventArgs e)
        {
            var sdr = new System.Windows.Forms.FolderBrowserDialog();
            if (sdr.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtDir.Text = sdr.SelectedPath;
                SelectdDir = sdr.SelectedPath;
            }
            else
            {
                txtDir.Text = null;
                SelectdDir = null;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = cbbList.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                SelectedTag = item.Tag.ToString();
            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int count;
            if (int.TryParse(tbCount.Text, out count))
            {
                if (count > 0)
                {
                    Count = count;
                    tbCount.Text = count.ToString();
                }
                else
                {
                    tbCount.Text = Count.ToString();
                }
            }
            else
            {
                tbCount.Text = Count.ToString();
            }
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == true)
            {
                txtFile.Text = sfd.FileName;
                SelectdFile = sfd.FileName;
            }
            else
            {
                txtFile.Text = null;
                SelectdFile = null;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(SelectdDir) && !string.IsNullOrEmpty(SelectdFile))
            {

                btnExport.IsEnabled = false;
                try
                {

                    var files = GetFiles(SelectdDir, "*.cs|*.xaml");

                    int writeCount = 0;

                    if (files != null && files.Length > 0)
                    {

                        foreach (var f in files)
                        {

                            tbTip.Text = $"正在提取{f}....";

                            if (writeCount < Count)
                            {
                                writeCount += WriteFile(f, SelectdFile);
                            }
                            else
                            {
                                break;
                            }

                            pPar.Value = writeCount * 1.0 / Count;
                        }
                    }
                }
                finally
                {
                    btnExport.IsEnabled = true;
                }
            }

        }

        private string[] GetFiles(string dir, string seachPattern = null)
        {
            var fs = new ArrayList();

            if (string.IsNullOrEmpty(seachPattern))
            {
                fs.AddRange(Directory.GetFiles(SelectdDir, "*.*", SearchOption.AllDirectories));
            }
            else
            {
                var ps = seachPattern.Split('|');
                foreach (var p in ps)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        fs.AddRange(Directory.GetFiles(SelectdDir, p, SearchOption.AllDirectories));
                    }
                }
            }

            return (string[])fs.ToArray(typeof(string));
        }


        private int WriteFile(string file, string target)
        {
            int line = 0;
            try
            {
                var lines = File.ReadAllLines(file);
                if (lines != null && lines.Length > 0)
                {
                    var als = lines.Where(l => (SelectedTag == "0" || !LanguageHelper.ExcludeCshap(l)) && !string.IsNullOrWhiteSpace(l));

                    File.AppendAllLines(target, als);

                    line = als.Count();
                }
            }
            catch { }
            return line;
        }
    }

    public class LanguageHelper
    {
        public static bool ExcludeCshap(string code)
        {
            return code.Contains("using") || code.Contains("enum");
        }

        public static bool Exclude(string code,string ltype = null)
        {
            if (string.IsNullOrEmpty(ltype))
            {
                return ExcludeCshap(code);
            }
            return true;
        }
    }
}
