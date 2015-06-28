using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml.Data;
using System.Linq;
using Windows.UI.Xaml.Media;
using InfoViewApp.InterestGathering.NewsFeed;
using Windows.UI.Xaml;

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

    class enum2LocalizedStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;//default now
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, value.ToString());
        }
    }

    public class newsSource2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var newsSource = value as FeedSource;
            if (newsSource == null) return Visibility.Collapsed;
            return newsSource.GetType() != typeof(CustomizedFeedSource) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    //accepts a boolean (and negates that) or a visibility value
    public class logicalNeg2Visibility:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                !(bool)value :
                (Visibility)value != Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    //accepts a boolean or a visibility value
    public class logical2Visibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                (bool)value :
                (Visibility)value == Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
