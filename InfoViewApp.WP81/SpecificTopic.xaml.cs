using InfoViewApp.WP81.InterestGathering;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SpecificTopic : PhoneApplicationPage
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
                progressRing.Visibility = Visibility.Visible;
                GoogleSpecificInterestGatherer gatherer = new GoogleSpecificInterestGatherer();
                SaveBtn.Visibility = Visibility.Collapsed;
                var interest = await gatherer.RequestContent(new InterestRequest()
                    {
                        InterestString = specificTopicBox.Text,
                        PreviousInterestContentIdentifier = 0
                    });
                SaveBtn.Visibility = Visibility.Visible;
                progressRing.Visibility = Visibility.Collapsed;
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
                NavigationService.Navigate(new System.Uri("/Preview.xaml", System.UriKind.Relative));
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
