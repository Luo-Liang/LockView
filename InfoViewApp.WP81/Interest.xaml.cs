﻿// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

using Microsoft.Phone.Controls;
using System.Windows;

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Interest : PhoneApplicationPage
    {
       
        public Interest()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (categorySelector.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                    NavigationService.Navigate(new System.Uri("/SpecificTopic.xaml", System.UriKind.Relative));
                    break;
                case 1:
                    NavigationService.Navigate(new System.Uri("/BroadInterestPage.xaml", System.UriKind.Relative));
                    break;
                case 2:
                    NavigationService.Navigate(new System.Uri("/WordsOfWisdom.xaml", System.UriKind.Relative));
                    break;
                default:
                    return;
            }
        }
    }
}
