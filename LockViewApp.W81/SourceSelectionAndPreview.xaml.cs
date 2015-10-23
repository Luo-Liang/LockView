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
            
        }

        private void SourceSelectionAndPreview_Loaded(object sender, RoutedEventArgs e)
        {
            RenderPreview();
            effectiveHeight = dtGrid.ActualHeight;
            RenderPreview();
            RescaleDTandPreviewGrid();
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
            RescaleDTandPreviewGrid();

        }
        void RescaleDTandPreviewGrid()
        {
            if (effectiveHeight == 0) return;//not loaded yet.
            var dtTransform = dtGrid.RenderTransform as CompositeTransform;
            //var previewTransform = previewItemStackPanel.RenderTransform as CompositeTransform;
            //dtTransform.ScaleX = dtTransform.ScaleY = previewTransform.ScaleX = previewTransform.ScaleY = 1;
            var scale = imagePreviewBox.ActualHeight * 0.15 / effectiveHeight;
            dtTransform.ScaleX = dtTransform.ScaleY = dtTransform.ScaleX *scale;
            //previewTransform.ScaleX = previewTransform.ScaleY = dtTransform.ScaleX * scale;
            effectiveHeight *= scale;
            foreach(PreviewItemDisplayControl ctrl in previewItemStackPanel.Children)
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
            if (dataContext.NavigationType == "specificInterest")
            {
                selectedControl = new SpecificInterestControl();
                selectedControl.SelectionStatusChanged += Control_SelectionStatusChanged;
                selectedControl.ShowMeClicked += Control_ShowMeClicked;
            }
            selectedControl.Width = setupTarget.ActualWidth;
            selectedControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            selectedControl.VerticalAlignment = VerticalAlignment.Top;
            setupTarget.Content = selectedControl;
        }

        private void Control_ShowMeClicked(object sender, GathererReadyEvent e)
        {
            var ctrl = requestRelationship[sender.GetType()];
            for (int i = 0; i < previewItemStackPanel.Children.Count; i++)
            {
                if (previewItemStackPanel.Children[i] == requestRelationship[sender.GetType()])
                {
                    ctrl.SelectedInterestIndex = i;
                    //update index i.
                    TemporaryContentStorage[i].CopyFromInterestContent(e.Content);
                    //assign. Let's just waste some processing time.
                    LockViewApplicationState.Instance.SelectedContextContracts = TemporaryContentStorage.ToArray();
                    break;
                }
            }
            ctrl.DataContext = LockViewApplicationState.Instance;
            RescaleDTandPreviewGrid();
        }
        List<OverlayContextContract> TemporaryContentStorage = new List<OverlayContextContract>();
        Dictionary<Type, PreviewItemDisplayControl> requestRelationship = new Dictionary<Type, PreviewItemDisplayControl>();
        private void Control_SelectionStatusChanged(object sender, InterestSelectionEvent e)
        {
            var preview = new PreviewItemDisplayControl();
            if (e.IsEnabled)
            {
                requestRelationship[sender.GetType()] = preview;
                previewItemStackPanel.Children.Add(preview);
                TemporaryContentStorage.Add(new OverlayContextContract());
            }
            else
            {
                TemporaryContentStorage.RemoveAt(previewItemStackPanel.Children.IndexOf(requestRelationship[sender.GetType()]));
                previewItemStackPanel.Children.Remove(requestRelationship[sender.GetType()]);
                requestRelationship.Remove(sender.GetType());
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
