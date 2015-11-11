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
    public sealed partial class CreateAStyleControl : UserControl
    {
        public CreateAStyleControl()
        {
            this.InitializeComponent();
        }

        public UIElement ReviveObject;

        private void saveStyle_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            if (ReviveObject != null)
            {
                ReviveObject.Visibility = Visibility.Visible;
            }
        }
    }
}
