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
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            StorageFile sf = StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appdata:///local/myfile.jpg")).GetAwaiter().GetResult();
            try
            {
                await LockScreen.SetImageStreamAsync(await sf.OpenReadAsync());
            }
            catch (Exception ex)
            {

            }
            deferral.Complete();
        }
    }
}
