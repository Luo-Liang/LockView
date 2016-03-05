using InfoViewApp.WP81;
using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.Tasks;
using LockViewApp.WP81.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LockViewApp.W81
{
    public partial class WeatherInterestControl : InterestGathererControl
    {
        public WeatherInterestControl()
        {
            this.InitializeComponent();
            this.Gatherer = new LockViewApp.WP81.Contracts.WeatherDataSource();
        }

        Geolocator geo = new Geolocator();
        HttpClient client = new HttpClient();
        WeatherDataSource gatherer
        {
            get { return this.Gatherer as WeatherDataSource; }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            busyBar.Visibility = Visibility.Visible;
            gatherer.CityName = textBox.Text;
            gatherer.IsImperial = useImperial.IsChecked.Value;
            gatherer.Language = LockViewApplicationState.Instance.RequestMetadata.RequestLanguage.Substring(0, 2);//<-- take language, not region.
            await InvokeContentRequestEvent(null);
            button.IsEnabled = true;
            busyBar.Visibility = Visibility.Collapsed;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

            button.IsEnabled = false;
            busyBar.Visibility = Visibility.Visible;
            Geoposition pos = null;
            try
            {
                pos = await geo.GetGeopositionAsync(); // get the raw geoposition data
            }
            catch
            {
                ((HyperlinkButton)sender).IsEnabled = false;
                return;
            }
            double lat = pos.Coordinate.Point.Position.Latitude; // current latitude
            double longt = pos.Coordinate.Point.Position.Longitude; // current 
            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.10240");
                string json = await client.GetStringAsync($"http://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={longt}");
                JsonObject jObj = null;
                textBox.Text = gatherer.DisplayName = "DEFAULT LOCATION";
                if (JsonObject.TryParse(json, out jObj))
                {
                    var addrObj = jObj["address"].GetObject();
                    textBox.Text = gatherer.DisplayName = addrObj["city"].GetString() + ", " + addrObj["country"].GetString();
                }
            }
            catch
            {
                textBox.Text = gatherer.DisplayName = "DEFAULT LOCATION";
            }
            gatherer.LongitudeAndLatitudeString = $"lat={lat}&lon={longt}";
            busyBar.Visibility = Visibility.Collapsed;
            button.IsEnabled = true;

        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = textBox.Text.Length != 0;
            InvokeSelectionStatusChange(true);

        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(false);
            button.IsEnabled = false;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            button.IsEnabled = false;
            if (textBox.Text.Length != 0)
            {
                button.IsEnabled = true;
            }
            (Gatherer as WeatherDataSource).LongitudeAndLatitudeString = null;
        }

    }
}
