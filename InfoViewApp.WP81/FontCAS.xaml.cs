// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

using Microsoft.Phone.Controls;

namespace InfoViewApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FontCAS : PhoneApplicationPage
    {
        public FontCAS()
        {
            this.InitializeComponent();
            var overlayFormattingContract = LockViewApplicationState.Instance.PreviewFormattingContract;
            settingsGrid.DataContext = overlayFormattingContract;

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
    }
}
