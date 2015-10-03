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
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                var source = new LockViewApp.WP81.Contracts.NASAAPODCaptionSource();
                LockViewApplicationState.Instance.SelectedProviders[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.NASAAPODFastTransitPage)] = source;
                var content = new OverlayContextContract();
                content.CopyFromInterestContent(await source.RequestContent(null));
                LockViewApplicationState.Instance.SelectedContextContracts[InterestNavigationQueue.Instance.GetNavigationSequence(InterestNavigationQueue.NASAAPODFastTransitPage)] = content;
                NavigationService.Navigate(InterestNavigationQueue.Instance.GetNextNavigationUri(InterestNavigationQueue.NASAAPODFastTransitPage));
            }
            else
            {
                //back.
                NavigationService.GoBack();
            }
        }
    }
}