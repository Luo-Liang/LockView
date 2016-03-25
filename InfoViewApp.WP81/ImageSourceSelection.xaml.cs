using InfoViewApp.WP81.Resources;
using Microsoft.Phone.Controls;
using System;
using System.Windows;

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
            var lbVM = new ListBoxContentVMCollection();
            lbVM.AddRange(new[] {
                new ListBoxContentVM() { FirstLine = AppResources.UseOwnImage, SecondLine = AppResources.UseOwnImageText,NavigationPath = new Uri("/ImageCropping.xaml?ImgSrc=library&padblack=false",UriKind.Relative) },
                new ListBoxContentVM() {FirstLine = AppResources.Bing,SecondLine = AppResources.BingText,NavigationPath = new Uri("/ImageSourceSettingsPage.xaml?ImgSrc=bing&padblack=false",UriKind.Relative) },
                new ListBoxContentVM() {FirstLine = AppResources.NASA, SecondLine = AppResources.NASAText,NavigationPath = new Uri("/ImageSourceSettingsPage.xaml?ImgSrc=nasa&padblack=false",UriKind.Relative) },
                new ListBoxContentVM() {FirstLine = AppResources.LiveEarth, SecondLine = AppResources.LiveEarthText,NavigationPath = new Uri("/ImageSourceSettingsPage.xaml?ImgSrc=le&padblack=true",UriKind.Relative) }
            });
            categorySelector.ItemsSource = lbVM;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (categorySelector.SelectedItem == null) return;
            var lbContext = categorySelector.SelectedItem as ListBoxContentVM;
            NavigationService.Navigate(lbContext.NavigationPath);
        }
    }
}
