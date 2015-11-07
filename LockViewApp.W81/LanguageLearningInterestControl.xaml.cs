using InfoViewApp.WP81.InterestGathering;
using InfoViewApp.WP81.InterestGathering.LanguageLearning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public partial class LanguageLearningInterestControl : InterestGathererControl
    {
        public LanguageLearningInterestControl()
        {
            this.InitializeComponent();
            this.Loaded += LanguageLearningInterestControl_Loaded;
        }

        private void LanguageLearningInterestControl_Loaded(object sender, RoutedEventArgs e)
        {
            var supportedLanguages = Enum.GetValues(typeof(LanguageSourceBase.LanguageType));
            var contentTypes = Enum.GetValues(typeof(LanguageSourceBase.ContentType));
            langugage.ItemsSource = supportedLanguages;
            translationLanguage.ItemsSource = supportedLanguages;
            Type.ItemsSource = contentTypes;
        }

        void Filter()
        {
            IEnumerable<LanguageSourceBase> fullSource = App.Current.Resources["definedLanguageSources"] as LanguegeSources;
            if (langugage.SelectedItem != null)
            {
                var languageType = (LanguageSourceBase.LanguageType)langugage.SelectedItem;
                fullSource = fullSource.Where<LanguageSourceBase>(source => source.Language == languageType);
            }
            if (translationLanguage.SelectedItem != null)
            {
                var languageType = (LanguageSourceBase.LanguageType)translationLanguage.SelectedItem;
                fullSource = fullSource.Where<LanguageSourceBase>(source => source.TranslationLanguage == languageType);
            }
            if (this.Type.SelectedItem != null)
            {
                var contentType = (LanguageSourceBase.ContentType)this.Type.SelectedItem;
                fullSource = fullSource.Where(source => source.Content == contentType);
            }
            SuggestedSource.ItemsSource = fullSource;
            SuggestedSource.SelectedItem = fullSource.FirstOrDefault();
            if (fullSource.Count() == 0)
            {
                button.IsEnabled = false;
            }
            else
            {
                button.IsEnabled = true;
            }
        }

        private void langugage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void translationLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(true);
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(false);
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            busyBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            this.Gatherer = SuggestedSource.SelectedItem as InterestGatherer;
            await InvokeContentRequestEvent(null);
            button.IsEnabled = true;
            busyBar.Visibility = Visibility.Collapsed;
        }
    }
}
