using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using TaskNOW;
using Windows.UI.Popups;
using Windows.UI.Notifications;

namespace TaskNOWBackground
{
    public sealed class TaskNOWBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            
            if (details != null)
            {
                string result
                SettingsManager.SetSetting<string>("DebugMessage", false, details.UserInput[);
            }
            await Task.Delay(5000);
            ToastManager.ShowToast();
            deferral.Complete();
        }
    }
}
