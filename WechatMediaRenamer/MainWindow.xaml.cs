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
using System.Threading;

namespace WechatMediaRenamer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] files;
        private bool useShotAt;

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
                useShotAt = cbShotAt.IsChecked ?? false;
                this.files = files;
                new Thread(new ThreadStart(RenameFiles)).Start();
            }
        }

        public void RenameFiles()
        {
            MediaFileRenamer mediaFileRenamer;
            StringBuilder sb = new StringBuilder();
            string renameMessage, originalContent = lbDrop.Dispatcher.Invoke(() => lbDrop.Content.ToString());
            int count = 0;
            foreach (var filePath in files)
            {
                mediaFileRenamer = MediaFileRenamer.FromFilePath(filePath);
                count++;
                Dispatcher.Invoke(() => lbDrop.Content = string.Format("Processing {0}/{1}", count, files.Length));
                if (!useShotAt)
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
                Dispatcher.Invoke(() => MessageBox.Show(sb.ToString()));
            }
            Dispatcher.Invoke(() => lbDrop.Content = originalContent);
        }
    }
}
