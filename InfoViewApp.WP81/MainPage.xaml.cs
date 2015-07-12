using Microsoft.Phone.Controls;
using System;
using System.Windows.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            //this.NavigationCacheMode = NavigationCacheMode.Required;
            this.Loaded += MainPage_Loaded1;
            LockViewApplicationState.Instance.RequestMetadata.ScaleFactor = ResolutionProvider.GetScaleFactor();
        }

        private void MainPage_Loaded1(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.Resources["load"] as Storyboard).Begin();
        }

        private void goToCropping_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ImageSourceSelection.xaml", UriKind.Relative));
        }

    }
}
