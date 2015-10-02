using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.Resources;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            currentConfig.Text = (InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.SpecificTopicPage)+1).ToString();
            totalConfigStep.Text = InterestNavigationQueue.Instance.NavigationPages.Count.ToString();
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
            if (SaveBtn.Content.ToString() == AppResources.ShowMe)
            {
                SaveBtn.Content = AppResources.Next;
                progressRing.Visibility = Visibility.Visible;
                GoogleSpecificInterestGatherer gatherer = new GoogleSpecificInterestGatherer();
                SaveBtn.Visibility = Visibility.Collapsed;
                LockViewApplicationState.Instance.SelectedInterests[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.SpecificTopicPage)] = new InterestRequest() { InterestString = specificTopicBox.Text };
                var interest = await gatherer.RequestContent(LockViewApplicationState.Instance.SelectedInterests[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.SpecificTopicPage)]);
                SaveBtn.Visibility = Visibility.Visible;
                progressRing.Visibility = Visibility.Collapsed;
                if (interest != null)
                {
                    InterestNavigationQueue.Instance.AssignContent(InterestNavigationQueue.SpecificTopicPage, interest);
                    previewStack.DataContext = interest;
                }
                else
                {
                    previewStack.DataContext = InterestContent.DefaultInterest;
                }
                InterestNavigationQueue.Instance.AssignProvider(InterestNavigationQueue.SpecificTopicPage, gatherer);
            }
            else
            {
                SaveBtn.Content = AppResources.ShowMe;
                NavigationService.Navigate(InterestNavigationQueue.Instance.GetNextNavigationUri(InterestNavigationQueue.SpecificTopicPage));
                //Navigate to customization page.
            }
        }

        private void specificTopicBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.Visibility = Visibility.Visible;
            SaveBtn.Content = AppResources.ShowMe;
            if (specificTopicBox.Text.Length == 0)
            {
                SaveBtn.Visibility =  Visibility.Collapsed;
            }
        }

        private void specificTopicBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus(); // dismiss the keyboard
                              // Call the submit method here
            }
        }
    }
}
