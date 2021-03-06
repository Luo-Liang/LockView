﻿using InfoViewApp.WP81.InterestGathering;
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

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpecificTopic : Page
    {
        public SpecificTopic()
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
            if (SaveBtn.Content.ToString() == "Show me!")
            {
                SaveBtn.Content = "Preview";
                progressRing.IsActive = true;
                GoogleSpecificInterestGatherer gatherer = new GoogleSpecificInterestGatherer();
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                var interest = await gatherer.RequestContent(new InterestRequest()
                    {
                        InterestString = specificTopicBox.Text,
                        PreviousInterestContentIdentifier = 0
                    });
                SaveBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                progressRing.IsActive = false;
                if (interest != null)
                {
                    LockViewApplicationState.Instance.PreviewContextContract.Title = interest.Title;
                    LockViewApplicationState.Instance.PreviewContextContract.FirstLine = interest.Content;
                    LockViewApplicationState.Instance.PreviewContextContract.SecondLine = interest.Publisher;
                    previewStack.DataContext = interest;
                }
                else
                {
                    previewStack.DataContext = InterestContent.DefaultInterest;
                }
                LockViewApplicationState.Instance.SelectedProvider = gatherer;
            }
            else
            {
                SaveBtn.Content = "Show me!";
                Frame.Navigate(typeof(Preview));
                //Navigate to customization page.
            }
        }

        private void specificTopicBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.IsEnabled = true;
            if (specificTopicBox.Text.Length == 0)
            {
                SaveBtn.Content = "Show me!";
                SaveBtn.IsEnabled = false;
            }
        }
    }
}
