using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;

namespace WechatMediaRenamer
{
    internal class MediaFileRenamer
    {
        public string FullFilePath { get; }
        public string FullDirectoryPath { get; }
        public string FileNameWithoutExtension { get; }
        public string Extension { get; }

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan chinaTimeSpan = new TimeSpan(8, 0, 0);
        private static readonly TimeZoneInfo chinaTime = TimeZoneInfo.CreateCustomTimeZone("CST", chinaTimeSpan, "China Standard SR Time", "SR Time");
        private static Regex r = new Regex(":");

        public static MediaFileRenamer FromFilePath(string filePath)
        {
            return new MediaFileRenamer(filePath);
        }

        public MediaFileRenamer(string filePath)
        {
            FullFilePath = filePath;
            FullDirectoryPath = Path.GetDirectoryName(filePath);
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            Extension = Path.GetExtension(filePath);
        }

        public string Rename()
        {
            string epochTime = GetDatePart(FileNameWithoutExtension);
            if (epochTime.Equals(string.Empty) || epochTime.Length != 13)
            {
                return string.Format("无法识别文件 '{0}'中的Epoch时间！", FullFilePath);
            }
            return RenameByDateTime(GetDateTimeInChinaFromEpochTime(epochTime));
        }

        public string Rename(bool useShotTime)
        {
            if (!useShotTime)
            {
                return Rename();
            }
            DateTime? shotAt = GetShotDate();
            if (shotAt == null)
            {
                return string.Format("'{0}' 不存在拍摄日期！", FullFilePath);
            }
            return RenameByDateTime((DateTime)shotAt);
        }

        private DateTime? GetShotDate()
        {
            using (FileStream fs = new FileStream(FullFilePath, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                try
                {
                    PropertyItem propItem = myImage.GetPropertyItem(0x9003);
                    string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
                    if (dateTaken.Length == 0)
                    {
                        return null;
                    }
                    else
                    {
                        return DateTime.Parse(dateTaken);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private string RenameByDateTime(DateTime dateTime)
        {
            string destPath = Path.Combine(FullDirectoryPath, ComposeNameFromEpochTime(dateTime));
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

        private string ComposeNameFromEpochTime(DateTime dateTime)
        {
            string dateTimePart = string.Format("{0:yyyyMMdd_HHmmss_fff}", dateTime);
            string fileNameTemplate = GetFileNameTemplateByExtention(Extension);
            return string.Format(fileNameTemplate, dateTimePart, Extension);
        }

        private DateTime GetDateTimeInChinaFromEpochTime(string epochTime)
        {
            DateTime utcDateTime = epoch.AddMilliseconds(long.Parse(epochTime));
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, chinaTime);
        }
    }
}
