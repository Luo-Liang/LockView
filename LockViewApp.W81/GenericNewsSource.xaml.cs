using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using InfoViewApp.WP81.InterestGathering.NewsFeed;
using InfoViewApp.WP81;
using InfoViewApp.WP81.InterestGathering;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LockViewApp.W81
{
    public partial class GenericNewsSource : InterestGathererControl
    {
        public GenericNewsSource()
        {
            this.InitializeComponent();
            sourceName.ItemsSource = (Application.Current.Resources["definedNewsFeedSources"] as FeedSources).Where<FeedSource>(o => o.ApplicableLanguageId == null || o.ApplicableLanguageId.ToLowerInvariant() == LockViewApplicationState.Instance.RequestMetadata.RequestLanguage.ToLowerInvariant());
            sourceName.SelectedIndex = 0;
        }

        private void sourceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sourceName.SelectedItem == null) return;
            button.IsEnabled = true;
            var src = sourceName.SelectedItem as FeedSource;
            if ((src).GetType() == typeof(CustomizedFeedSource))
            {
                customizedGrid.Visibility = Visibility.Visible;
                nonCustomized.Visibility = Visibility.Collapsed;
                if (rssField.Text.Trim() == string.Empty)
                {
                    button.IsEnabled = false;
                }
            }
            else
            {
                customizedGrid.Visibility = Visibility.Collapsed;
                nonCustomized.Visibility = Visibility.Visible;
            }
            categoryName.ItemsSource = src.FeedContentProviders;
            categoryName.SelectedIndex = 0;
        }

        private void rssField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rssField.Text.Trim() == string.Empty)
            {
                button.IsEnabled = false;
            }
            else
            {
                button.IsEnabled = true;
            }
            var customSrc = categoryName.SelectedItem as NewsFeedCategory;
            customSrc.XmlSource = rssField.Text;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            busyBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            Gatherer = categoryName.SelectedItem as InterestGatherer;
            await InvokeContentRequestEvent(null);
            busyBar.Visibility = Visibility.Collapsed;
            button.IsEnabled = true;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(true);
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(false);
        }
    }
}
