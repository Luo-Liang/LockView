using LockViewApp.WP81.Contracts;
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
    public partial class WordOfWisdomInterestControl : InterestGathererControl
    {
        public WordOfWisdomInterestControl()
        {
            this.InitializeComponent();
            Gatherer = new SingleTextSource();
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
            var singleTextSource = Gatherer as SingleTextSource;
            singleTextSource.Title = title.Text;
            singleTextSource.FirstLine = content.Text;
            singleTextSource.SecondLine = footnote.Text;
            await InvokeContentRequestEvent(new InfoViewApp.WP81.InterestGathering.InterestRequest() { });
        }
    }
}
