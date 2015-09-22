// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

using InfoViewApp.WP81.Resources;
using Microsoft.Phone.Controls;
using System.Windows;
using static InfoViewApp.WP81.InterestNavigationQueue;

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
            var lbm = new ListBoxContentVMCollection();
            lbm.AddRange(new ListBoxContentVM[]
            {
                new ListBoxContentVM() {FirstLine = AppResources.SpecificTopicOfYourChoice, SecondLine = AppResources.SpecificTopicOfYourChoiceText,NavigationPath = SpecificTopicPage },
                new ListBoxContentVM() {FirstLine = AppResources.GenericNewsTopic,SecondLine = AppResources.GenericNewsTopicText ,NavigationPath= InterestNavigationQueue.BroadInterestPage},
                new ListBoxContentVM() {FirstLine = AppResources.WordOfWisdom,SecondLine = AppResources.WordOfWisdomText ,NavigationPath=WordOfWisdomPage },
                new ListBoxContentVM() {FirstLine = AppResources.LanguageLearning,SecondLine = AppResources.LanguageLearningText,NavigationPath = LanguageSettingPage }
            });
            categorySelector.ItemsSource = lbm;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (categorySelector.SelectedItem == null) return;
            Instance.NavigationPages.Clear();
            for (int i = 0; i < categorySelector.SelectedItems.Count; i++)
            {
                Instance.NavigationPages.Add((categorySelector.SelectedItems[i] as ListBoxContentVM).NavigationPath);
            }
            LockViewApplicationState.Instance.SelectedProviders = new InterestGathering.InterestGatherer[categorySelector.SelectedItems.Count];
            LockViewApplicationState.Instance.SelectedContextContracts = new OverlayContextContract[categorySelector.SelectedItems.Count];
            NavigationService.Navigate(Instance.NavigationPages[0]);
        }
    }
}
