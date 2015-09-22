using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfoViewApp.WP81
{
    class InterestNavigationQueue
    {
        public static InterestNavigationQueue Instance { get; private set; }
        private InterestNavigationQueue() { }
        static InterestNavigationQueue()
        {
            Instance = new InterestNavigationQueue();
        }
        public static Uri LanguageSettingPage { get; } = new Uri("/LanguageSetting.xaml", UriKind.Relative);
        public static Uri BroadInterestPage { get; } = new Uri("/BroadInterestPage.xaml", UriKind.Relative);
        public static Uri SpecificTopicPage { get; } = new Uri("/SpecificTopic.xaml", UriKind.Relative);
        public static Uri WordOfWisdomPage { get; } = new Uri("/WordOfWisdom.xaml", UriKind.Relative);
        public static Uri PreviewPage { get; } = new Uri("/Preview.xaml", UriKind.Relative);
        public List<Uri> NavigationPages { get; set; } = new List<Uri>();
        public int GetNavigationSequence(Uri currentPageUri) => NavigationPages.IndexOf(currentPageUri);
        public Uri GetNextNavigationUri(Uri currentPageUri) => NavigationPages.IndexOf(currentPageUri) + 1 == NavigationPages.Count ? PreviewPage : NavigationPages[GetNavigationSequence(currentPageUri)]; 
    }
}
