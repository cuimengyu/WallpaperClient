using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WallpaperClient.Services;

namespace WallpaperClient.Converters
{
    /// <summary>
    /// 布尔值到可见性转换器
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// 反向布尔值到可见性转换器
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return true;
        }
    }

    /// <summary>
    /// 反向布尔值转换器
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// 数字到可见性转换器（零则隐藏）
    /// </summary>
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is double doubleValue)
            {
                return doubleValue == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 数字到可见性转换器（非零则显示）
    /// </summary>
    public class NonZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is double doubleValue)
            {
                return doubleValue != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 字符串到可见性转换器（空字符串则隐藏）
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrEmpty(stringValue) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 文件大小格式化转换器
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long bytes)
            {
                return FormatFileSize(bytes);
            }
            if (value is int intBytes)
            {
                return FormatFileSize(intBytes);
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// 日期时间格式化转换器
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            DateTime? dateTime = value as DateTime?;
            if (dateTime == null || !dateTime.HasValue)
                return string.Empty;

            var format = parameter as string;
            if (string.IsNullOrEmpty(format))
                format = "yyyy-MM-dd HH:mm:ss";

            return dateTime.Value.ToString(format, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
            return null!;
        }
    }

    /// <summary>
    /// 相对时间转换器
    /// </summary>
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            DateTime? dateTime = value as DateTime?;
            if (dateTime == null || !dateTime.HasValue)
                return string.Empty;

            return GetRelativeTime(dateTime.Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalSeconds < 60)
            {
                return "刚刚";
            }
            if (timeSpan.TotalMinutes < 60)
            {
                return $"{(int)timeSpan.TotalMinutes} 分钟前";
            }
            if (timeSpan.TotalHours < 24)
            {
                return $"{(int)timeSpan.TotalHours} 小时前";
            }
            if (timeSpan.TotalDays < 7)
            {
                return $"{(int)timeSpan.TotalDays} 天前";
            }
            if (timeSpan.TotalDays < 30)
            {
                return $"{(int)(timeSpan.TotalDays / 7)} 周前";
            }
            if (timeSpan.TotalDays < 365)
            {
                return $"{(int)(timeSpan.TotalDays / 30)} 个月前";
            }
            return $"{(int)(timeSpan.TotalDays / 365)} 年前";
        }
    }

    /// <summary>
    /// 百分比转换器
    /// </summary>
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return $"{doubleValue:F1}%";
            }
            if (value is int intValue)
            {
                return $"{intValue}%";
            }
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (stringValue.EndsWith("%"))
                {
                    stringValue = stringValue.TrimEnd('%');
                }
                if (double.TryParse(stringValue, out var result))
                {
                    return result;
                }
            }
            return 0;
        }
    }

    /// <summary>
    /// 枚举到布尔值转换器
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return false;
            }

            var enumValue = value.ToString();
            var targetValue = parameter.ToString();

            return enumValue == targetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null!;
            }

            var isTrue = (bool)value;
            if (isTrue)
            {
                var enumType = targetType.IsEnum ? targetType : Nullable.GetUnderlyingType(targetType);
                var parameterString = parameter.ToString();
                if (enumType != null && !string.IsNullOrEmpty(parameterString) && Enum.IsDefined(enumType, parameterString))
                {
                    return Enum.Parse(enumType, parameterString);
                }
            }
            return null!;
        }
    }

    /// <summary>
    /// 多值到可见性转换器（所有值都为true时显示）
    /// </summary>
    public class AllTrueToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 多值到可见性转换器（任一值为true时显示）
    /// </summary>
    public class AnyTrueToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool boolValue && boolValue)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Null到可见性转换器
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 非Null到可见性转换器
    /// </summary>
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 颜色转换器（字符串到Brush）
    /// </summary>
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString && !string.IsNullOrEmpty(colorString))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new System.Windows.Media.SolidColorBrush(color);
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.SolidColorBrush brush)
            {
                return brush.Color.ToString();
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// 数值范围转换器
    /// </summary>
    public class RangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string rangeString)
            {
                var parts = rangeString.Split('-');
                if (parts.Length == 2 && double.TryParse(parts[0], out var min) && double.TryParse(parts[1], out var max))
                {
                    return doubleValue >= min && doubleValue <= max;
                }
            }
            if (value is int intValue && parameter is string intRangeString)
            {
                var parts = intRangeString.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out var min) && int.TryParse(parts[1], out var max))
                {
                    return intValue >= min && intValue <= max;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 图片 URL 优先级转换器
    /// 优先使用本地文件 LocalPath，如果不存在则使用中等分辨率 ThumbnailUrl，然后是原图 Url，最后使用 SmallUrl
    /// </summary>
    public class ImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value 应该是 Wallpaper 对象
            if (value is Models.Wallpaper wallpaper)
            {
                // 优先使用本地文件（如果存在）
                if (!string.IsNullOrWhiteSpace(wallpaper.LocalPath) && System.IO.File.Exists(wallpaper.LocalPath))
                {
                    return wallpaper.LocalPath;
                }

                // 如果没有本地文件，使用缩略图（中等分辨率，加载速度适中）
                if (!string.IsNullOrWhiteSpace(wallpaper.ThumbnailUrl))
                {
                    return wallpaper.ThumbnailUrl;
                }
                // 如果缩略图为空，使用原图 URL（最高分辨率）
                if (!string.IsNullOrWhiteSpace(wallpaper.Url))
                {
                    return wallpaper.Url;
                }
                // 最后使用小图（低分辨率）
                if (!string.IsNullOrWhiteSpace(wallpaper.SmallUrl))
                {
                    return wallpaper.SmallUrl;
                }
            }

            // 如果直接传入字符串 URL
            if (value is string url && !string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 下载状态到可见性转换器
    /// </summary>
    public class DownloadStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DownloadState state && parameter is string action)
            {
                return action switch
                {
                    "CanPause" => state == DownloadState.Downloading || state == DownloadState.Paused
                        ? Visibility.Visible : Visibility.Collapsed,
                    "IsDownloading" => state == DownloadState.Downloading
                        ? Visibility.Visible : Visibility.Collapsed,
                    "IsPaused" => state == DownloadState.Paused
                        ? Visibility.Visible : Visibility.Collapsed,
                    "CanCancel" => state == DownloadState.Downloading || state == DownloadState.Paused || state == DownloadState.Pending
                        ? Visibility.Visible : Visibility.Collapsed,
                    "CanRemove" => state == DownloadState.Completed || state == DownloadState.Failed || state == DownloadState.Cancelled
                        ? Visibility.Visible : Visibility.Collapsed,
                    _ => Visibility.Collapsed
                };
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
