using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using Windows.Foundation;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageSourceSelection : PhoneApplicationPage
    {
        public ImageSourceSelection()
        {
            this.InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (categorySelector.SelectedItem == null) return;
            if (categorySelector.SelectedIndex == 0) NavigationService.Navigate(new Uri("/ImageCropping.xaml?ImgSrc=library", UriKind.Relative));
            else NavigationService.Navigate(new Uri("/ImageCropping.xaml?ImgSrc=bing", UriKind.Relative));
        }
    }
}
