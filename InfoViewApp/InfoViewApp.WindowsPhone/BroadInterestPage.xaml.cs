using InfoViewApp.InterestGathering;
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

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var collection = App.Current.Resources["definedNewsFeedSources"] as FeedSources;

            if (SaveBtn.Content as string == "Show me!")
            {
                InterestContent interestContent = InterestContent.DefaultInterest;
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                progressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //show me needs to get info.
                if (newsSources.SelectedItem.GetType() == typeof(CustomizedFeedSource))
                {
                    interestContent = await LockViewApplicationState.Instance.SelectedProvider.RequestContent(null);
                }
                else
                {
                    var provider = newsTopic.SelectedItem as IInterestGatherer;
                    interestContent = await provider.RequestContent(null);
                }
                previewStack.DataContext = interestContent;
                LockViewApplicationState.Instance.PreviewContextContract.Title = interestContent.Title;
                LockViewApplicationState.Instance.PreviewContextContract.FirstLine = interestContent.Content;
                LockViewApplicationState.Instance.PreviewContextContract.SecondLine = interestContent.Publisher;
                SaveBtn.Content = "Preview";
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                progressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {

                SaveBtn.Content = "Show me!";
                Frame.Navigate(typeof(Preview));
            }
        }
        private void newsTopic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveBtn.Content = "Show me!";
            if ((newsSources.SelectedItem as FeedSource).GetType() != typeof(CustomizedFeedSource))
                if ((sender as ComboBox).SelectedIndex != -1)
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void RssField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.Content = "Show me!";
            if ((newsSources.SelectedItem as FeedSource).GetType() == typeof(CustomizedFeedSource))
            {
                if ((sender as TextBox).Text.Length > 0)
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                var customSource = newsSources.SelectedItem as CustomizedFeedSource;
                LockViewApplicationState.Instance.SelectedProvider = customSource.FeedContentProviders[0];
            }
        }

        private void newsSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveBtn.Content = "Show me!";
            //TODO:: Needs translation
            SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
