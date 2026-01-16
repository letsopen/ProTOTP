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
                switch (colorName.ToLower())
                {
                    case "green":
                        return Colors.Green;
                    case "red":
                        return Colors.Red;
                    case "orange":
                        return Colors.Orange;
                    case "blue":
                        return Colors.Blue;
                    case "yellow":
                        return Colors.Yellow;
                    case "purple":
                        return Colors.Purple;
                    default:
                        return Colors.Gray;
                }
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}