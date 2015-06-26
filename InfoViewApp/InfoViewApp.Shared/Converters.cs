using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace InfoViewApp.Converter
{
    class Str2BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var clrCollection = App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var color = clrCollection.FirstOrDefault<ColorNameVM>(clr => clr.ColorName.Replace(" ", "") == value.ToString());
            return new SolidColorBrush(color.Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var clrObj = value as SolidColorBrush;
            var clrCollection = App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var clrNameRetrieval = clrCollection.FirstOrDefault<ColorNameVM>(o => o.Color == clrObj.Color);
            return clrNameRetrieval;
        }
    }
    class Str2ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var clrCollection = App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var color = clrCollection.FirstOrDefault<ColorNameVM>(clr => clr.ColorName == value.ToString());
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == default(ColorNameVM)) return "White";
            var clrObj = value as ColorNameVM;
            return clrObj.ColorName.Replace(" ", "");
        }
    }

    class int2FontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var fontContract = (FontContract)value;
            return fontContract.FontSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return new FontContract()
            {
                FontFamily = "Segoe UI",
                FontSize = int.Parse(value.ToString())
            };
        }
    }
}
