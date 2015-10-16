using InfoViewApp.WP81;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LockViewApp.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.SizeChanged += MainPage_SizeChanged;
        }
        private async void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ViewManagementHelper.DetermineFullScreen() == false)
            {
                maximizeAnimationGrid.Visibility = Visibility.Visible;
                maximize.Begin();
                croppingGuide.Text = "Maximize this app first";
                croppingGuideText.Text = "In order to provide a best fit for your lock screen, you need to preview the picture in maximize window mode. Maximize your window now to continue.";
                //cannot process the request. User not in full screen mode.
                //imageCropper.Source = await bitmap.FromContent(new Uri("ms-appx:///Assets/Maximize.png"),Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8);
            }
            else
            {
                croppingGuide.Text = "Pick a best view";
                croppingGuideText.Text = "When you've decided where to get your picture, you can crop the picture to fit your screen. Move around and pick a best view!";
                maximize.Stop();
                maximizeAnimationGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem;
            if (selectedItem == null)
            {
                nextButton.IsEnabled = false;
                return;
            }
            var context = selectedItem as ListBoxContentVM;
           
           
        }
    }
}
