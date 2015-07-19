using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.InterestGathering.LanguageLearning;
using InfoViewApp.WP81.InterestGathering.NewsFeed;
using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace InfoViewApp.WP81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LanguageSetting : PhoneApplicationPage
    {
        public LanguageSetting()
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
            var supportedLanguages = Enum.GetValues(typeof(LanguageSourceBase.LanguageType));
            var contentTypes = Enum.GetValues(typeof(LanguageSourceBase.ContentType));
            newsSources.ItemsSource = supportedLanguages;
            newsTopic.ItemsSource = supportedLanguages;
            languageType.ItemsSource = contentTypes;
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SaveBtn.Content as string == "Show me!")
            {
                SaveBtn.Visibility = Visibility.Collapsed;
                progressRing.Visibility = Visibility.Visible;
                InterestContent interestContent = await (languageSource.SelectedItem as IInterestGatherer).RequestContent(LockViewApplicationState.Instance.SelectedInterest);
                previewStack.DataContext = interestContent;
                LockViewApplicationState.Instance.PreviewContextContract.CopyFromInterestContent(interestContent);
                SaveBtn.Content = "Preview";
                SaveBtn.Visibility = Visibility.Visible;
                progressRing.Visibility = Visibility.Collapsed;
                LockViewApplicationState.Instance.SelectedProvider = languageSource.SelectedItem as InterestGatherer;
            }
            else
            {

                SaveBtn.Content = "Show me!";
                NavigationService.Navigate(new Uri("/Preview.xaml", UriKind.Relative));
            }
        }


        void Filter()
        {
            if (newsSources == null || newsTopic == null) return;
            IEnumerable<LanguageSourceBase> fullSource = WP81.App.Current.Resources["definedLanguageSources"] as LanguegeSources;
            if (newsSources.SelectedItem != null)
            {
                var languageType = (LanguageSourceBase.LanguageType)newsSources.SelectedItem;
                fullSource = fullSource.Where<LanguageSourceBase>(source => source.Language == languageType);
            }
            if (newsTopic.SelectedItem != null)
            {
                var languageType = (LanguageSourceBase.LanguageType)newsTopic.SelectedItem;
                fullSource = fullSource.Where<LanguageSourceBase>(source => source.TranslationLanguage == languageType);
            }
            if (this.languageType.SelectedItem != null)
            {
                var contentType = (LanguageSourceBase.ContentType)this.languageType.SelectedItem;
                fullSource = fullSource.Where<LanguageSourceBase>(source => source.Content == contentType);
            }
            languageSource.ItemsSource = fullSource;
            if (fullSource.Count() == 0)
            {
                SaveBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                SaveBtn.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Invalid Name. Ignore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newsSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }
        /// <summary>
        /// Invalid Name. Ignore
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newsTopic_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void languageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void languageSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SaveBtn!=null)
            SaveBtn.Visibility = Visibility.Visible;
        }
    }
}
