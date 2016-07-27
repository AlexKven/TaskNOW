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
            ToastManager.ShowInteractiveToast();
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (details != null)
            {
                var error = await CreateTaskFromTriggerDetails(details);
                if (error != null)
                    ToastManager.ShowMessageToast("Error Creating Task", error);
            }
            deferral.Complete();
        }

        private async Task<string> CreateTaskFromTriggerDetails(ToastNotificationActionTriggerDetail details)
        {
            int priority = int.Parse(details.Argument.Last().ToString());
            string description = details.UserInput["content"].ToString();
            int projectId = int.Parse(details.UserInput["project"].ToString());
            int dateId = int.Parse(details.UserInput["date"].ToString());
            int timeId = int.Parse(details.UserInput["time"].ToString());
            var dateFun = PresetManager.GetDatePresets().First(prs => prs.Item1 == dateId).Item3;
            var timeFun = PresetManager.GetTimePresets().First(prs => prs.Item1 == timeId).Item3;
            int numParams = (dateFun == null ? 1 : 0) + (timeFun == null ? 1 : 0) + 1;
            var userParams = ParseParams(description, numParams, ',');
            if (userParams.Last() == null)
            {
                return "No description was provided.";
            }
            DateTime time;
            DateTime date;
            if (timeFun == null)
            {
                if (!DateTime.TryParse(userParams[dateFun == null ? 1 : 0].Trim(), out time))
                {
                    return userParams[dateFun == null ? 1 : 0].Trim() + " is not a valid time.";
                }
            }
            else
                time = timeFun(DateTime.Now);
            if (dateFun == null)
            {
                if (!DateTime.TryParse(userParams[0].Trim(), out date))
                {
                    return userParams[0].Trim() + " is not a valid date.";
                }
                date = PresetManager.SetTime(date, time);
            }
            else
                date = dateFun(DateTime.Now, time);
            
            var token = SettingsManager.GetSetting<string>("Token", true, null);

            var code = await HttpRequests.CreateTask(projectId, date, userParams.Last(), 5 - priority, token);
            while (code == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                code = await HttpRequests.CreateTask(projectId, date, userParams.Last(), 5 - priority, token);
            }

            switch (code)
            {
                case System.Net.HttpStatusCode.Forbidden:
                    ToastManager.ShowMessageToast("Request Forbidden (403)", "For some strange reason, we received a 403 forbidden response from the server.");
                    break;
                case System.Net.HttpStatusCode.BadRequest:
                    ToastManager.ShowMessageToast("Bad Request (400)", "We sent a bad HTTP request. This shouldn't happen, and there is probably a bug in the app.");
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    ToastManager.ShowMessageToast("Not Found (404)", "Our request to the server gave us a 404 error. This shouldn't happen, and there is probably a bug in the app.");
                    break;
                case System.Net.HttpStatusCode.InternalServerError:
                    ToastManager.ShowMessageToast("Server Error (500)", "We sent a bad HTTP request. This shouldn't happen, and there is probably a bug in the app.");
                    break;
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    ToastManager.ShowMessageToast("Service Unavailable (503)", "The Todoist server is unavailable (it's not your internet connection). Try again later.");
                    break;
            }

            //ToastManager.ShowMessageToast("Task Created", userParams.Last().Trim() + " at " + date.ToString());
            //SettingsManager.SetSetting<string>("DebugMessage", false, userParams.Last() + " at " + date.ToString());
            return null;
        }

        private static string[] ParseParams(string s, int numParams, char delimeter)
        {
            string[] result = new string[numParams];
            int curParam = 0;
            foreach (var chr in s)
            {
                if (chr == delimeter && curParam < numParams - 1)
                    curParam++;
                else
                {
                    if (result[curParam] == null)
                        result[curParam] = chr.ToString();
                    else
                        result[curParam] += chr;
                }
            }
            return result;
        }
    }
}
