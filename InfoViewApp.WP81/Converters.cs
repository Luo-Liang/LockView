using InfoViewApp.WP81.InterestGathering.NewsFeed;
using InfoViewApp.WP81;
using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace InfoViewApp.WP81.Converter
{
    public class Str2BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clrCollection = WP81.App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var color = clrCollection.FirstOrDefault<ColorNameVM>(clr => clr.ColorName.Replace(" ", "") == value.ToString());
            return new SolidColorBrush(color.Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            var clrObj = value as SolidColorBrush;
            var clrCollection = App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var clrNameRetrieval = clrCollection.FirstOrDefault<ColorNameVM>(o => o.Color == clrObj.Color);
            return clrNameRetrieval;
        }
    }
    public class Str2ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var clrCollection = WP81.App.Current.Resources["colorCollection"] as ColorNameVMCollection;
            var color = clrCollection.FirstOrDefault<ColorNameVM>(clr => clr.ColorName == value.ToString());
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == default(ColorNameVM)) return "White";
            var clrObj = value as ColorNameVM;
            return clrObj.ColorName.Replace(" ", "");
        }

    }
    public class int2FontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fontContract = (FontContract)value;
            return fontContract.FontSize / ResolutionProvider.GetScaleFactor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FontContract()
            {
                FontFamily = "Segoe UI",
                FontSize = (int)(int.Parse(value.ToString()) * ResolutionProvider.GetScaleFactor())
            };
        }

    }
    public class enum2LocalizedStrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetName(value.GetType(),value);//default now
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, value.ToString());
        }

    }
    public class newsSource2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var newsSource = value as FeedSource;
            if (newsSource == null) return Visibility.Collapsed;
            return newsSource.GetType() != typeof(CustomizedFeedSource) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    //accepts a boolean (and negates that) or a visibility value
    public class logicalNeg2Visibility:IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                !(bool)value :
                (Visibility)value != Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    //accepts a boolean or a visibility value
    public class logical2Visibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                (bool)value :
                (Visibility)value == Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    public class str2LocStr : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
