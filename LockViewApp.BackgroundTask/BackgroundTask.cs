using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.Background;

namespace LockViewApp.BackgroundTask
{
    public sealed class UpdateLockScreenBackgroundTask:IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            deferral.Complete();
        }
    }
}
