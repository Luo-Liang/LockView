using InfoViewApp.WP81;
using InfoViewApp.WP81.InterestGathering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
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
    public sealed partial class PreviewItemDisplayControl : UserControl
    {
        public PreviewItemDisplayControl()
        {
            InitializeComponent();
            DataContextChanged += PreviewItemDisplayControl_DataContextChanged;
        }

        public int SelectedInterestIndex { get; set; } = -1;
        private void PreviewItemDisplayControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var value = args.NewValue as LockViewApplicationState;
            if (value == null) return;
            TitleTextBox.Text = value.SelectedContextContracts[SelectedInterestIndex].Title;
            ContentTextBox.Text = value.SelectedContextContracts[SelectedInterestIndex].FirstLine;
            PublisherTextBox.Text = value.SelectedContextContracts[SelectedInterestIndex].SecondLine;
        }

        public void RescaleContent(double cumulatedScaleFactor)
        {
            TitleTextBox.FontSize *= cumulatedScaleFactor;
            ContentTextBox.FontSize *= cumulatedScaleFactor;
             PublisherTextBox.FontSize *= cumulatedScaleFactor;
        }
    }
}
