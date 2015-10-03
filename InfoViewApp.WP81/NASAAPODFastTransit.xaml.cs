using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace InfoViewApp.WP81
{
    public partial class NASAAPODFastTransit : PhoneApplicationPage
    {
        public NASAAPODFastTransit()
        {
            InitializeComponent();
            this.Loaded += NASAAPODFastTransit_Loaded;
         
        }

        private async void NASAAPODFastTransit_Loaded(object sender, RoutedEventArgs e)
        {
            var source = new LockViewApp.WP81.Contracts.NASAAPODCaptionSource();
            LockViewApplicationState.Instance.SelectedProviders[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.NASAAPODFastTransitPage)] = source;
            var content = new OverlayContextContract();
            content.CopyFromInterestContent(await source.RequestContent(null));
            LockViewApplicationState.Instance.SelectedContextContracts[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.NASAAPODFastTransitPage)] = content;
            NavigationService.Navigate(InterestNavigationQueue.Instance.GetNextNavigationUri(InterestNavigationQueue.NASAAPODFastTransitPage));
        }
    }
}