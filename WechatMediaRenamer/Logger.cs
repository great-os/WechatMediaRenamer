using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WechatMediaRenamer
{
    public class Logger
    {
        internal static void LogString(string input)
        {
            string logPath = AppDomain.CurrentDomain.BaseDirectory + "logfile.txt"; // 日志文件路径，以应用程序所在目录为基础

            try
            {
                using (StreamWriter writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine(DateTime.Now.ToString() + ": " + input); // 写入当前时间和输入参数到日志文件
                }

                Console.WriteLine("日志已写入文件: " + logPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("写入日志时出现错误: " + ex.Message);
            }
        }
    }
}
