using InfoViewApp.WP81.InterestGathering;
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
    public sealed partial class SpecificInterestControl : InterestGathererControl
    {
        public SpecificInterestControl()
        {
            this.InitializeComponent();
            Gatherer = new GoogleSpecificInterestGatherer();
        }

        async private void button_Click(object sender, RoutedEventArgs e)
        {
            busyBar.Visibility = Visibility.Visible;
            button.IsEnabled = false;
            await InvokeContentRequestEvent(new InterestRequest() { InterestString = textBox.Text});
            busyBar.Visibility = Visibility.Collapsed;
            button.IsEnabled = true;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(true);
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InvokeSelectionStatusChange(false);
        }
    }
}
