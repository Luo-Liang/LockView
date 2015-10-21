using InfoViewApp.WP81.InterestGathering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LockViewApp.W81
{
    public class GathererReadyEvent : EventArgs
    {
        public InterestContent Content { get; private set; }
        public GathererReadyEvent(InterestContent content)
        {
            Content = content;
        }
    }

    public class InterestSelectionEvent : EventArgs
    {
        public bool IsEnabled { get; private set; }
        public InterestSelectionEvent(bool selected)
        {
            IsEnabled = selected;
        }
    }

    public class InterestGathererControl : UserControl
    {
        public InterestGatherer Gatherer { get; set; }
        public event EventHandler<GathererReadyEvent> ShowMeClicked;
        public event EventHandler<InterestSelectionEvent> SelectionStatusChanged;
        //unclear if this is a good design.
        //can probably use DependencyProperty instead.
        async internal Task InvokeContentReadyEvent(InterestRequest request)
        {
            if (ShowMeClicked != null)
            {
                ShowMeClicked(this, new GathererReadyEvent(await Gatherer.RequestContent(request)));
            }
        }

        internal void InvokeSelectionStatusChange(bool selected)
        {
            if (SelectionStatusChanged != null)
            {
                SelectionStatusChanged(this, new InterestSelectionEvent(selected));
            }
        }
    }
}
