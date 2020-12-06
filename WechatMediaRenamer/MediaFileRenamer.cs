using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace WechatMediaRenamer
{
    class MediaFileRenamer
    {
        public string FullFilePath { get; }
        public string FullDirectoryPath { get; }
        public string FileNameWithoutExtension { get; }
        public string Extension { get; }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan chinaTimeSpan = new TimeSpan(8, 0, 0);
        private static readonly TimeZoneInfo chinaTime = TimeZoneInfo.CreateCustomTimeZone("CST", chinaTimeSpan, "China Standard SR Time", "SR Time");

        public static MediaFileRenamer FromFilePath(string filePath)
        {
            return new MediaFileRenamer(filePath);
        }

        public MediaFileRenamer(string filePath)
        {
            this.FullFilePath = filePath;
            this.FullDirectoryPath = Path.GetDirectoryName(filePath);
            this.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            this.Extension = Path.GetExtension(filePath);
        }

        public string Rename()
        {
            string epochTime = GetDatePart(this.FileNameWithoutExtension);
            if (epochTime.Equals(String.Empty) || epochTime.Length != 13)
            {
                return String.Format("无法识别文件 '{0}'中的Epoch时间！", this.FullFilePath);
            }
            string destPath = Path.Combine(this.FullDirectoryPath, ComposeNameFromEpochTime(epochTime));
            if (File.Exists(destPath))
            {
                return String.Format("'{0}' 无法被重命名为 '{1}'，因为目标已存在！", this.FullFilePath, destPath);
            }
            File.Move(this.FullFilePath, destPath);
            return String.Empty;
        }

        private string GetDatePart(string fileNameWithoutExtention)
        {
            string datePattern = @"[^\d]*(\d+)";
            Match m = Regex.Match(fileNameWithoutExtention, datePattern, RegexOptions.IgnoreCase);
            if (m.Success && m.Groups.Count > 1)
                return m.Groups[1].Value;
            return String.Empty;
        }

        private string GetFileNameTemplateByExtention(string extName)
        {
            switch (extName.ToLower())
            {
                case ".mp4":
                    return "VID_{0}{1}";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                    return "IMG_{0}{1}";
                default:
                    return "{0}{1}";
            }
        }

        private string ComposeNameFromEpochTime(string epochTime)
        {
            string dateTimePart = String.Format("{0:yyyyMMdd_HHmmss}", GetDateTimeInChinaFromEpochTime(epochTime));
            string fileNameTemplate = GetFileNameTemplateByExtention(this.Extension);
            return String.Format(fileNameTemplate, dateTimePart, this.Extension);
        }

        private DateTime GetDateTimeInChinaFromEpochTime(string epochTime)
        {
            DateTime utcDateTime = epoch.AddMilliseconds(long.Parse(epochTime));
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, chinaTime);
        }
    }
}
