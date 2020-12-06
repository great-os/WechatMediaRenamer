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
using System.IO;
using System.Text.RegularExpressions;

namespace WechatMediaRenamer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan chinaTimeSpan = new TimeSpan(8, 0, 0);
        private static readonly TimeZoneInfo chinaTime = TimeZoneInfo.CreateCustomTimeZone("CST", chinaTimeSpan, "China Standard SR Time", "SR Time");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                RenameFiles(files);
            }
        }

        private void RenameFiles(string[] files)
        {
            MediaFileRenamer mediaFileRenamer;
            StringBuilder sb = new StringBuilder();
            string renameMessage;
            foreach (var filePath in files)
            {
                mediaFileRenamer = MediaFileRenamer.FromFilePath(filePath);
                if (!(cbShotAt.IsChecked ?? false))
                {
                    renameMessage = mediaFileRenamer.Rename();
                }
                else
                {
                    renameMessage = mediaFileRenamer.Rename(true);
                }
                if (renameMessage.Length > 0)
                {
                    sb.AppendLine(renameMessage);
                }
            }
            if (sb.Length > 0)
            {
                MessageBox.Show(sb.ToString());
            }
        }
    }
}
