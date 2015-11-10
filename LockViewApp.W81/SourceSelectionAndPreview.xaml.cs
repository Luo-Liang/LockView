using InfoViewApp.WP81;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LockViewApp.W81
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SourceSelectionAndPreview : Page
    {
        public SourceSelectionAndPreview()
        {
            this.InitializeComponent();
            this.SizeChanged += SourceSelectionAndPreview_SizeChanged;
            this.Loaded += SourceSelectionAndPreview_Loaded;
            this.mockScreen.SizeChanged += ImagePreviewBox_SizeChanged;

        }

        private void ImagePreviewBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RescaleDTandPreviewGrid(e.NewSize.Height);
        }

        private void SourceSelectionAndPreview_Loaded(object sender, RoutedEventArgs e)
        {
            RenderPreview();
            effectiveHeight = dtGrid.ActualHeight;
            RescaleDTandPreviewGrid(mockScreen.ActualHeight);
        }

        WriteableBitmap selectedImage;
        double effectiveHeight = 0;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            selectedImage = e.Parameter as WriteableBitmap;
            imageCropper.Source = selectedImage;
            //RenderPreview();

        }

        private void SourceSelectionAndPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderPreview();
        }
        void RescaleDTandPreviewGrid(double height)
        {
            if (effectiveHeight == 0) return;//not loaded yet.
            var dtTransform = dtGrid.RenderTransform as CompositeTransform;
            //var previewTransform = previewItemStackPanel.RenderTransform as CompositeTransform;
            //dtTransform.ScaleX = dtTransform.ScaleY = previewTransform.ScaleX = previewTransform.ScaleY = 1;
            var scale = height * 0.1 / effectiveHeight;
            dtTransform.ScaleX = dtTransform.ScaleY = dtTransform.ScaleX * scale;
            //previewTransform.ScaleX = previewTransform.ScaleY = dtTransform.ScaleX * scale;
            effectiveHeight *= scale;
            foreach (PreviewItemDisplayControl ctrl in previewItemStackPanel.Children)
            {
                ctrl.RescaleContent(scale);
            }

        }
        void RenderPreview()
        {
            var height = LockViewApplicationState.Instance.PreviewLayoutContract.TargetHeight;
            var width = LockViewApplicationState.Instance.PreviewLayoutContract.TargetWidth;
            var boundingBoxhwRatio = (boundingBox.ActualHeight - 50) / (boundingBox.ActualWidth - 30);
            var actualScreenRatio = 1.0 * height / width;
            if (actualScreenRatio > boundingBoxhwRatio)
            {
                //align height.
                mockScreen.Height = boundingBox.ActualHeight - 50;
                mockScreen.Width = width * (boundingBox.ActualHeight - 50) / height;
            }
            else
            {
                mockScreen.Width = boundingBox.ActualWidth - 30;
                mockScreen.Height = height * (boundingBox.ActualWidth - 30) / width;
            }
            //mockScreen.Height = imageViewBox.Height;
            //mockScreen.Width = imageViewBox.Width;
            //imageCropper.Height = imageViewBox.Height;
            //imageCropper.Width = imageViewBox.Width;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem == null) return;
            var dataContext = listBox.SelectedItem as ListBoxContentVM;
            setupTarget.ClearValue(ScrollViewer.ContentProperty);
            InterestGathererControl selectedControl = null;

            if (!navigationRelatiobship.ContainsKey(dataContext.NavigationType))
            {
                if (dataContext.NavigationType == "specificInterest")
                {
                    navigationRelatiobship[dataContext.NavigationType] = new SpecificInterestControl();
                }
                else if (dataContext.NavigationType == "weather")
                {
                    navigationRelatiobship[dataContext.NavigationType] = new WeatherInterestControl();
                }
                else if (dataContext.NavigationType == "news")
                {
                    navigationRelatiobship[dataContext.NavigationType] = new GenericNewsSource();
                }
                else if(dataContext.NavigationType == "language")
                {
                    navigationRelatiobship[dataContext.NavigationType] = new LanguageLearningInterestControl();
                }
                else if(dataContext.NavigationType == "wordofwisdom")
                {
                    navigationRelatiobship[dataContext.NavigationType] = new WordOfWisdomInterestControl();
                }
                navigationRelatiobship[dataContext.NavigationType].SelectionStatusChanged += Control_SelectionStatusChanged;
                navigationRelatiobship[dataContext.NavigationType].ShowMeClicked += Control_ShowMeClicked;
            }
            selectedControl = navigationRelatiobship[dataContext.NavigationType];
            selectedControl.Width = setupTarget.ActualWidth;
            selectedControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            selectedControl.VerticalAlignment = VerticalAlignment.Top;
            setupTarget.Content = selectedControl;
        }

        private void Control_ShowMeClicked(object sender, GathererReadyEvent e)
        {
            var ctrl = requestRelationship[sender as InterestGathererControl];
            for (int i = 0; i < previewItemStackPanel.Children.Count; i++)
            {
                if (previewItemStackPanel.Children[i] == requestRelationship[sender as InterestGathererControl])
                {
                    ctrl.SelectedInterestIndex = i;
                    //update index i.
                    TemporaryContentStorage[i].CopyFromInterestContent(e.Content);
                    TemporaryInterestStorage[i] = e.Request;
                    //assign. Let's just waste some processing time.
                    LockViewApplicationState.Instance.SelectedContextContracts = TemporaryContentStorage.ToArray();
                    LockViewApplicationState.Instance.SelectedInterests = TemporaryInterestStorage.ToArray();
                    break;
                }
            }
            ctrl.DataContext = LockViewApplicationState.Instance;
            RescaleDTandPreviewGrid(mockScreen.ActualHeight);
        }
        List<OverlayContextContract> TemporaryContentStorage = new List<OverlayContextContract>();
        List<InterestRequest> TemporaryInterestStorage = new List<InterestRequest>();
        Dictionary<InterestGathererControl, PreviewItemDisplayControl> requestRelationship = new Dictionary<InterestGathererControl, PreviewItemDisplayControl>();
        Dictionary<string, InterestGathererControl> navigationRelatiobship = new Dictionary<string, InterestGathererControl>();
        private void Control_SelectionStatusChanged(object sender, InterestSelectionEvent e)
        {
            var preview = new PreviewItemDisplayControl();
            if (e.IsEnabled)
            {
                requestRelationship[sender as InterestGathererControl] = preview;
                previewItemStackPanel.Children.Add(preview);
                TemporaryContentStorage.Add(new OverlayContextContract());
                TemporaryInterestStorage.Add(new InterestRequest());
            }
            else
            {
                var idx = previewItemStackPanel.Children.IndexOf(requestRelationship[sender as InterestGathererControl]);
                TemporaryContentStorage.RemoveAt(idx);
                previewItemStackPanel.Children.Remove(requestRelationship[sender as InterestGathererControl]);
                requestRelationship.Remove(sender as InterestGathererControl);
                TemporaryInterestStorage.RemoveAt(idx);
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            LockViewApplicationState.Instance.SelectedProviders = requestRelationship.Keys.Cast<InterestGathererControl>().Select(o => o.Gatherer).ToArray();
            this.Frame.Navigate(typeof(ReadyPage));
        }
    }
}
