﻿using System;
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
    public sealed partial class Interest : Page
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (categorySelector.SelectedIndex)
            {
                case -1:
                    return;
                case 0:
                    Frame.Navigate(typeof(SpecificTopic));
                    break;
                case 1:
                    Frame.Navigate(typeof(BroadInterestPage));
                    break;
                case 2:
                    Frame.Navigate(typeof(LanguageSetting));
                    break;
                default:
                    return;
            }
        }
    }
}
