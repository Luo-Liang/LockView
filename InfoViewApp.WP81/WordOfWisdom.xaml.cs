using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using LockViewApp.WP81.Contracts;
using static InfoViewApp.WP81.InterestNavigationQueue;
namespace InfoViewApp.WP81
{
    public partial class WordOfWisdom : PhoneApplicationPage
    {
        public WordOfWisdom()
        {
            InitializeComponent();
            var dataContext = new SingleTextSource();
            //update sources.
            Instance.AssignProvider(WordOfWisdomPage, dataContext);
            LayoutRoot.DataContext = dataContext;
            currentConfig.Text = (Instance.GetNavigationSequence(InterestNavigationQueue.WordOfWisdomPage)+1).ToString();
            totalConfigStep.Text = Instance.NavigationPages.Count.ToString();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var context = new OverlayContextContract();
            context.CopyFromInterestContent(await LockViewApplicationState.Instance.SelectedProviders[Instance.GetNavigationSequence(WordOfWisdomPage)].RequestContent(null));
            Instance.AssignContent(WordOfWisdomPage, context);
            NavigationService.Navigate(Instance.GetNextNavigationUri(WordOfWisdomPage));
        }
    }
}