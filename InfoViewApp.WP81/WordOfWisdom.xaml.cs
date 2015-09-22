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
namespace InfoViewApp.WP81
{
    public partial class WordOfWisdom : PhoneApplicationPage
    {
        public WordOfWisdom()
        {
            InitializeComponent();
            LayoutRoot.DataContext = LockViewApplicationState.Instance.SelectedProvider =  new SingleTextSource();
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            LockViewApplicationState.Instance.PreviewContextContract = new OverlayContextContract();
            LockViewApplicationState.Instance.PreviewContextContract.CopyFromInterestContent(await LockViewApplicationState.Instance.SelectedProvider.RequestContent(null));
            NavigationService.Navigate(InterestNavigationQueue.Instance.GetNextNavigationUri(InterestNavigationQueue.WordOfWisdomPage));
        }
    }
}