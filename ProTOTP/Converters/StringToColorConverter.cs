using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ProTOTP.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string colorName)
            {
                // 简单判断目标类型，避免复杂的反射操作
                var color = Colors.Gray; // 默认颜色
                
                switch (colorName.ToLower())
                {
                    case "green":
                        color = Colors.Green;
                        break;
                    case "red":
                        color = Colors.Red;
                        break;
                    case "orange":
                        color = Colors.Orange;
                        break;
                    case "blue":
                        color = Colors.Blue;
                        break;
                    case "yellow":
                        color = Colors.Yellow;
                        break;
                    case "purple":
                        color = Colors.Purple;
                        break;
                    default:
                        color = Colors.Gray;
                        break;
                }
                
                // 对于ProgressBar（SolidColorBrush.Color），返回Color
                // 对于TextBlock（TextBlock.Foreground），返回Brush
                // 我们可以通过参数来区分，或者简单地总是返回Brush
                // 因为SolidColorBrush可以接受Color作为参数，但TextBlock.Foreground需要Brush
                // 为了兼容两者，我们可以检查参数
                
                // 如果参数包含特定标识，表示需要Color而非Brush
                if (parameter != null && parameter.ToString() == "Color")
                {
                    return color;
                }
                else
                {
                    // 默认返回Brush，适用于TextBlock.Foreground
                    return new SolidColorBrush(color);
                }
            }
            // 默认返回Brush
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}