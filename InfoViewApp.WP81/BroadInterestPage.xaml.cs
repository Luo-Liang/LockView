﻿using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.InterestGathering.NewsFeed;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BroadInterestPage : PhoneApplicationPage
    {
        public BroadInterestPage()
        {
            this.InitializeComponent();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var collection = WP81.App.Current.Resources["definedNewsFeedSources"] as FeedSources;

            if (SaveBtn.Content as string == "Show me!")
            {
                InterestContent interestContent = InterestContent.DefaultInterest;
                SaveBtn.Visibility = Visibility.Collapsed;
                progressRing.Visibility = Visibility.Visible;
                //show me needs to get info.
                if (newsSources.SelectedItem.GetType() == typeof(CustomizedFeedSource))
                {
                    interestContent = await LockViewApplicationState.Instance.SelectedProvider.RequestContent(LockViewApplicationState.Instance.SelectedInterest);
                }
                else
                {
                    LockViewApplicationState.Instance.SelectedProvider = newsTopic.SelectedItem as InterestGatherer;
                    var provider = newsTopic.SelectedItem as IInterestGatherer;
                    interestContent = await provider.RequestContent(LockViewApplicationState.Instance.SelectedInterest);
                }
                previewStack.DataContext = interestContent;
                LockViewApplicationState.Instance.PreviewContextContract.CopyFromInterestContent(interestContent);
                SaveBtn.Content = "Preview";
                SaveBtn.Visibility = Visibility.Visible;
                progressRing.Visibility = Visibility.Collapsed;
            }
            else
            {

                SaveBtn.Content = "Show me!";
                NavigationService.Navigate(new Uri("/Preview.xaml", UriKind.Relative));
            }
        }
        private void newsTopic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveBtn.Content = "Show me!";
            if ((newsSources.SelectedItem as FeedSource).GetType() != typeof(CustomizedFeedSource))
                if ((sender as ListPicker).SelectedIndex != -1)
                    SaveBtn.Visibility = Visibility.Visible;
                else
                    SaveBtn.Visibility = Visibility.Collapsed;
        }

        private void RssField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.Content = "Show me!";
            if ((newsSources.SelectedItem as FeedSource).GetType() == typeof(CustomizedFeedSource))
            {
                if ((sender as TextBox).Text.Length > 0)
                    SaveBtn.Visibility = Visibility.Visible;
                else
                    SaveBtn.Visibility = Visibility.Collapsed;
                var customSource = newsSources.SelectedItem as CustomizedFeedSource;
                LockViewApplicationState.Instance.SelectedProvider = customSource.FeedContentProviders[0];
            }
        }

        private void newsSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SaveBtn == null) return;
            SaveBtn.Content = "Show me!";
            //TODO:: Needs translation
            SaveBtn.Visibility = Visibility.Collapsed;
        }

        private void rssField_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus(); // dismiss the keyboard
                              // Call the submit method here
            }
        }
    }
}