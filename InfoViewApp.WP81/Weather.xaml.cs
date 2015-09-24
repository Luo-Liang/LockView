using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using InfoViewApp.WP81.Resources;
using InfoViewApp.WP81.InterestGathering;
using LockViewApp.WP81.Contracts;

namespace InfoViewApp.WP81
{
    public partial class Weather : PhoneApplicationPage
    {
        public Weather()
        {
            InitializeComponent();
            currentConfig.Text = (InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.WeatherSettingsPage) + 1).ToString();
            totalConfigStep.Text = InterestNavigationQueue.Instance.NavigationPages.Count.ToString();
        }
        WeatherDataSource source = new WeatherDataSource();
        private void weatherBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }

        private void weatherBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.Content = AppResources.ShowMe;
            SaveBtn.Visibility = Visibility.Collapsed;
            if(weatherBox.Text.Length != 0)
            {
                SaveBtn.Visibility = Visibility.Visible;
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveBtn.Content as string == AppResources.ShowMe)
            {
                SaveBtn.Visibility = Visibility.Collapsed;
                progressRing.Visibility = Visibility.Visible;
                source.CityName = weatherBox.Text;
                source.IsImperial = useImperial.IsChecked.Value;
                source.Language = LockViewApplicationState.Instance.RequestMetadata.RequestLanguage.Substring(0, 2);//<-- take language, not region.
                InterestContent interestContent = await source.RequestContent(null);
                previewStack.DataContext = interestContent;
                InterestNavigationQueue.Instance.AssignContent(InterestNavigationQueue.WeatherSettingsPage, interestContent);
                SaveBtn.Content = AppResources.Next;
                SaveBtn.Visibility = Visibility.Visible;
                progressRing.Visibility = Visibility.Collapsed;
                InterestNavigationQueue.Instance.AssignProvider(InterestNavigationQueue.WeatherSettingsPage,source);
            }
            else
            {
                SaveBtn.Content = AppResources.ShowMe;
                NavigationService.Navigate(InterestNavigationQueue.Instance.GetNextNavigationUri(InterestNavigationQueue.WeatherSettingsPage));
            }
        }
    }
}