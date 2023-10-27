using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using MetadataExtractor;
using System.Collections.Generic;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;

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

        public string RenameByShotAt()
        {
            DateTime? shotAt = GetShotDate();
            if (shotAt == null)
            {
                return string.Format("'{0}' 不存在拍摄日期！", FullFilePath);
            }
            return RenameByDateTime((DateTime)shotAt);
        }

        public string Rename(ProcessMode processMode)
        {
            switch (processMode)
            {
                case ProcessMode.Default:
                    return Rename();
                case ProcessMode.ShotAt:
                    return RenameByShotAt();
                case ProcessMode.UpdatedAt:
                    return RenameByDateTime(GetUpdatedAt());
                case ProcessMode.CreatedAt:
                    return RenameByDateTime(GetCreateAt());
                default:
                    return String.Empty;
            }
        }

        private DateTime? GetShotDate()
        {
            DateTime? shotDate = null;
            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(FullFilePath);
                Print(directories);
                foreach (var directory in directories)
                {
                    if (directory is ExifSubIfdDirectory)
                    {
                        var subIfdDirectory = directory as ExifSubIfdDirectory;
                        if (subIfdDirectory.ContainsTag(ExifDirectoryBase.TagDateTimeOriginal))
                        {
                            shotDate = subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                            break; // Exit the loop since we found the shot date
                        }
                        if (subIfdDirectory.ContainsTag(ExifDirectoryBase.TagDateTimeDigitized))
                        {
                            shotDate = subIfdDirectory.GetDateTime(ExifDirectoryBase.TagDateTime);
                            break; // Exit the loop since we found the shot date
                        }
                    }
                    if (directory is QuickTimeMovieHeaderDirectory)
                    {
                        var subDirectory = directory as QuickTimeMovieHeaderDirectory;
                        if (subDirectory.ContainsTag(QuickTimeMovieHeaderDirectory.TagCreated))
                        {
                            shotDate = subDirectory.GetDateTime(QuickTimeMovieHeaderDirectory.TagCreated);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Logger.LogString(ex.Message);
                Console.WriteLine(ex.Message);
            }
            return shotDate;
        }

        // Write all extracted values to stdout
        static void Print(IEnumerable<MetadataExtractor.Directory> directories)
        {
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine();
            foreach (var directory in directories)
            {
                Console.WriteLine($"-------{directory.GetType()}-------");
                foreach (var tag in directory.Tags)
                {
                    Console.WriteLine($"{directory.Name} - {tag.Name}({tag.Type}) = {tag.Description}");
                    Console.WriteLine($" Value: {directory.GetType()}");
                }
            }

        }

        private string _GetDateStringFromProperty(Image image, int propertyId)
        {
            if (!Array.Exists(image.PropertyIdList, id => id == propertyId)) return null;
            PropertyItem propItem = image.GetPropertyItem(propertyId);
            return r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
        }

        private DateTime GetCreateAt()
        {
            return File.GetCreationTime(FullFilePath);
        }

        private DateTime GetUpdatedAt()
        {
            return File.GetLastWriteTime(FullFilePath);
        }

        private string RenameByDateTime(DateTime dateTime)
        {
            string destPath = Path.Combine(FullDirectoryPath, ComposeNameFromEpochTime(dateTime, false));
            if (FullFilePath.Equals(destPath))
            {
                // 文件名未改变
                return String.Empty;
            }
            if (File.Exists(destPath))
            {
                // try with miliseconds
                destPath = Path.Combine(FullDirectoryPath, ComposeNameFromEpochTime(dateTime, true));
            }
            if (FullFilePath.Equals(destPath))
            {
                // 文件名未改变
                return String.Empty;
            }
            if (File.Exists(destPath))
            {
                // alert duplicate
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
                case ".avi":
                case ".mov":
                    return "VID_{0}{1}";
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".webp":
                    return "IMG_{0}{1}";
                default:
                    return "{0}{1}";
            }
        }

        private string ComposeNameFromEpochTime(DateTime dateTime, bool includeMiliseconds)
        {
            string template = includeMiliseconds ? "{0:yyyyMMdd_HHmmss_fff}" : "{0:yyyyMMdd_HHmmss}";
            string dateTimePart = string.Format(template, dateTime);
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
