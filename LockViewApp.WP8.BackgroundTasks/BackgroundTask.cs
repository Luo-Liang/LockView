using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.UserProfile;

namespace LockViewApp.W81.BackgroundTasks
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            StorageFile sf = StorageFile.GetFileFromPathAsync("myfile.jpg").GetAwaiter().GetResult();
            LockScreen.SetImageFileAsync(sf).GetAwaiter().GetResult();
        }
    }
}
