using InfoViewApp.InterestGathering.NewsFeed;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BroadInterestPage : Page
    {
        public BroadInterestPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void newsTopic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((newsSources.SelectedItem as FeedSource).GetType() != typeof(CustomizedFeedSource))
                if ((sender as ComboBox).SelectedIndex != -1)
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void RssField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((newsSources.SelectedItem as FeedSource).GetType() == typeof(CustomizedFeedSource))
            {
                if ((sender as TextBox).Text.Length > 0)
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            }
        }

        private void newsSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //TODO:: Needs translation
            SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
