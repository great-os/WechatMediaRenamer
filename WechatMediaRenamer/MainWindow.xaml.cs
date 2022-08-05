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
        private System.Timers.Timer timer;
        private ProcessMode processMode = ProcessMode.Default;

        public MainWindow()
        {
            InitializeComponent();
            this.timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(() => { LabelColorChanger(); });
            };
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Ellipse_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                this.files = (string[])e.Data.GetData(DataFormats.FileDrop);
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
                renameMessage = mediaFileRenamer.Rename(processMode);
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

        private void rbCommandMode_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (sender as RadioButton);
            switch (rb.Name)
            {
                case "rbDefault":
                    processMode = ProcessMode.Default;
                    break;
                case "rbShotAt":
                    processMode = ProcessMode.ShotAt;
                    break;
                case "rbCreatedAt":
                    processMode = ProcessMode.CreatedAt;
                    break;
                case "rbUpdateAt":
                    processMode = ProcessMode.UpdatedAt;
                    break;
                default:
                    break;
            }
        }

        private void LabelColorChanger()
        {
            Random rnd = new Random();
            lb52pojie.Foreground = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255), (byte)rnd.Next(1, 255)));
        }
    }
}
