using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace LockViewApp.W81
{
    class ViewManagementHelper
    {
        public static bool DetermineFullScreen()
        {
            return ApplicationView.GetForCurrentView().IsFullScreen;
        }
    }
}
