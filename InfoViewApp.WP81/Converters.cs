using InfoViewApp.WP81.InterestGathering.NewsFeed;
using InfoViewApp.WP81;
using System;
using System.Linq;
using System.Globalization;
using System.Windows;
#if WINDOWS_PHONE
using InfoViewApp.WP81.Resources;
using System.Windows.Data;
using System.Windows.Media;
#elif WINDOWS_APP
#endif
using System.Collections.Generic;
using System.Reflection;
#if WINDOWS_APP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using LockViewApp.W81;
#endif

namespace InfoViewApp.WP81.Converter
{
    public class Str2BrushConverter : IValueConverter
    {
        static Dictionary<string, Brush> colorCache;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (colorCache == null)
            {
#if WINDOWS_PHONE
                colorCache = (App
#elif WINDOWS_APP
                colorCache = (Application
#endif
                .Current.Resources["colorCollection"] as ColorNameVMCollection).ToDictionary<ColorNameVM, string, Brush>(o => o.ColorName.Replace(" ", ""), o => new SolidColorBrush(o.Color));
            }
            return colorCache[value.ToString()];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            var clrObj = value as SolidColorBrush;
            var clrCollection =
#if WINDOWS_PHONE
                App
#elif WINDOWS_APP
                Application
#endif
                .Current.Resources["colorCollection"] as ColorNameVMCollection;
            var clrNameRetrieval = clrCollection.FirstOrDefault<ColorNameVM>(o => o.Color == clrObj.Color);
            return clrNameRetrieval;
        }
    }
    public class Str2ColorConverter : IValueConverter
    {
        static Dictionary<string, ColorNameVM> colorCache;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //race condition can be tolerated because assignment is atomic.
            if (colorCache == null)
            {
                var clrCollection =
#if WINDOWS_PHONE
                    WP81.App
#elif WINDOWS_APP
                    Application
#endif
                    .Current.Resources["colorCollection"] as ColorNameVMCollection;
                colorCache = clrCollection.ToDictionary(o => o.ColorName);
            }
            return colorCache[value.ToString()];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
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
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 1.333 * (int)value / ResolutionProvider.GetScaleFactor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new FontContract()
            {
                FontFamily = "Segoe UI",
                FontSize = (int)(int.Parse(value.ToString()) * ResolutionProvider.GetScaleFactor() * 0.75)
            };
        }

    }
    public class enum2LocalizedStrConverter : IValueConverter
    {
        static Dictionary<string, string> propertyCache;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if WINDOWS_PHONE
            if (propertyCache == null)
            {
                // propertyCache = typeof(AppResources).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).ToDictionary<PropertyInfo, string, string>(o => o.Name, o => o.GetValue(null).ToString());
                // the above is hard to debug.
                propertyCache = new Dictionary<string, string>();
                foreach (var property in typeof(AppResources).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    propertyCache[property.Name] = property.GetValue(null) as string;
                }
            }
            return propertyCache.ContainsKey(value.ToString()) ? propertyCache[value.ToString()] : Enum.GetName(value.GetType(), value);//default now
#elif WINDOWS_APP
            return value; // do not convert for now.
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.Parse(targetType, value.ToString());
        }

    }
    public class newsSource2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            var newsSource = value as FeedSource;
            if (newsSource == null) return Visibility.Collapsed;
            return newsSource.GetType() != typeof(CustomizedFeedSource) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    //accepts a boolean (and negates that) or a visibility value
    public class logicalNeg2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                !(bool)value :
                (Visibility)value != Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    //accepts a boolean or a visibility value
    public class logical2Visibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            bool isVisible = value.GetType() == typeof(bool) ?
                (bool)value :
                (Visibility)value == Visibility.Visible;
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ConvertBack(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
    public class str2LocStr : IValueConverter
    {
        static Dictionary<string, string> propertyCache;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, CultureInfo.CurrentCulture);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
#if WINDOWS_PHONE
            //again, allow race.
            if (propertyCache == null)
            {
                // propertyCache = typeof(AppResources).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).ToDictionary<PropertyInfo, string, string>(o => o.Name, o => o.GetValue(null).ToString());
                // the above is hard to debug.
                propertyCache = new Dictionary<string, string>();
                foreach (var property in typeof(AppResources).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                {
                    propertyCache[property.Name] = property.GetValue(null) as string;
                }
            }
            var key = value.ToString().Replace(" ", "");
            return propertyCache.ContainsKey(key) ? propertyCache[key] : key;
#elif WINDOWS_APP
            return value; //no translation for now.
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
