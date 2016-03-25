using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Maps.Services;

namespace InfoViewApp.WP81
{
    public partial class ImageSourceSettingsPage : PhoneApplicationPage
    {
        Geolocator geolocator;
        public ImageSourceSettingsPage()
        {
            InitializeComponent();
            geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.Default;
            geolocator.MovementThreshold = 50;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CannotGetLocationGrid.Visibility == Visibility.Visible)
            {
                var result = new[] { radioButtonWestern, radioButtonEastern, radioButtonNeutral }.Where(o => o.IsChecked.Value).Select(o => o.Content.ToString().ToLowerInvariant()).FirstOrDefault();
                //user override
                var padblack = NavigationContext.QueryString["padblack"];
                LockViewApplicationState.Instance.SelectedImageSourceParameters = $"location={result}&padblack={padblack}";
                NavigationService.Navigate(new Uri($"/ImageCropping.xaml?ImgSrc={NavigationContext.QueryString["ImgSrc"]}", UriKind.Relative));
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busyGrid.Visibility = Visibility.Visible;
                var location = await geolocator.GetGeopositionAsync();
                var eastern = location.Coordinate.Point.Position.Longitude > 0;
                busyGrid.Visibility = Visibility.Collapsed;
                CannotGetLocationGrid.Visibility = Visibility.Collapsed;
            }
            catch
            {
                CannotGetLocationGrid.Visibility = Visibility.Visible;
            }

        }
    }
}