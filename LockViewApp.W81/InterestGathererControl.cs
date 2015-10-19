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
        public IInterestGatherer InvokingGatherer { get; private set; }
        public GathererReadyEvent(IInterestGatherer gatherer)
        {
            InvokingGatherer = gatherer;
        }
    }
    public class InterestGathererControl : UserControl
    {
        public InterestGatherer Gatherer { get; set; }
        public event EventHandler<GathererReadyEvent> ShowMeClicked;
        internal void InvokeHandlerIfPossible()
        {
            if (ShowMeClicked != null)
            {
                ShowMeClicked(this, new GathererReadyEvent(Gatherer));
            }
        }
    }
}
