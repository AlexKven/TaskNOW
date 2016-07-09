using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace TaskNOW
{
    static class ToastManager
    {
        public static void ShowToast()
        {
            string template = CreateToastTemplate();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(template);
            ToastNotification toast = new ToastNotification(doc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private static string CreateToastTemplateNoPresets(bool noProjectPresets, bool noDatePresets, bool noTimePresets)
        {
            if (!(noProjectPresets || noTimePresets || noDatePresets))
                throw new ArgumentException("At least one of the arguments must be true.");
            List<string> problems = new List<string>();
            if (noProjectPresets)
                problems.Add("projects");
            if (noDatePresets)
                problems.Add("dates");
            if (noTimePresets)
                problems.Add("times");
            StringBuilder result = new StringBuilder();
            result.Append("<toast>"
                    + "<visual>"
                    + "<binding template = \"ToastGeneric\" >"
                    + "<text>Can't Create Task Generator</text>"
                    + "<text>There are no preset selections for ");
            for (int i = 0; i < problems.Count; i++)
            {
                result.Append(problems[i]);
                if (i == 1 && problems.Count == 3)
                    result.Append(", or ");
                if (i == 0 && problems.Count == 3)
                    result.Append(", ");
                if (i == 0 && problems.Count == 2)
                    result.Append(" or ");
            }
            result.Append(". Open TaskNOW to fix it.</text>"
                    + "</binding>"
                    + "</visual>"
                    + "</toast>");

            return result.ToString();
        }

        private static string CreateToastTemplate()
        {
            List<Tuple<int, string>> projectPresets = new List<Tuple<int, string>>();
            List<Tuple<int, string>> datePresets = new List<Tuple<int, string>>();
            List<Tuple<int, string>> timePresets = new List<Tuple<int, string>>();
            var totalProjects = PresetManager.GetProjectPresets();
            var totalDates = PresetManager.GetDatePresets();
            var totalTimes = PresetManager.GetTimePresets();
            for (int i = 0; i < 5; i++)
            {
                int presetId = SettingsManager.GetSetting<int>("ProjectPreset" + i, true, -1);
                if (presetId != -1)
                    projectPresets.Add(new Tuple<int, string>(presetId, totalProjects.FirstOrDefault(proj => proj.Item1 == presetId)?.Item2 ?? "(Unknown Project)"));
                presetId = SettingsManager.GetSetting<int>("DatePreset" + i, true, -1);
                if (presetId != -1)
                    datePresets.Add(new Tuple<int, string>(presetId, totalDates.FirstOrDefault(date => date.Item1 == presetId)?.Item2 ?? "(Error)"));
                presetId = SettingsManager.GetSetting<int>("TimePreset" + i, true, -1);
                if (presetId != -1)
                    timePresets.Add(new Tuple<int, string>(presetId, totalTimes.FirstOrDefault(time => time.Item1 == presetId)?.Item2 ?? "(Error)"));
            }
            if (projectPresets.Count == 0 || datePresets.Count == 0 || timePresets.Count == 0)
                return CreateToastTemplateNoPresets(projectPresets.Count == 0, datePresets.Count == 0, timePresets.Count == 0);
            StringBuilder result = new StringBuilder();
            result.Append("<toast>"
                    + "<visual>"
                    + "<binding template = \"ToastGeneric\" >"
                    + "<text>Create Todoist Task</text>"
                    + "<text>Expand to quickly create a task:</text>"
                    + "</binding>"
                    + "</visual>"
                    + "<actions>"
                    + $"<input id=\"project\" type=\"selection\" defaultInput=\"{projectPresets[0].Item1}\">");
            for (int i = 0; i < projectPresets.Count; i++)
            {
                result.Append("<selection id=\"");
                result.Append(projectPresets[i].Item1);
                result.Append("\" content=\"");
                result.Append(projectPresets[i].Item2);
                result.Append("\"/>");
            }
                    //+ "<selection id=\"0\" content=\"Yes\"/>"
                    //+ "<selection id=\"-1\" content=\"No\"/>"
            result.Append("</input>"
                    + $"<input id=\"date\" type=\"selection\" defaultInput=\"{datePresets[0].Item1}\">");
            for (int i = 0; i < datePresets.Count; i++)
            {
                result.Append("<selection id=\"");
                result.Append(datePresets[i].Item1);
                result.Append("\" content=\"");
                result.Append(datePresets[i].Item2);
                result.Append("\"/>");
            }
            result.Append("</input>"
                    + $"<input id=\"time\" type=\"selection\" defaultInput=\"{timePresets[0].Item1}\">");
            for (int i = 0; i < timePresets.Count; i++)
            {
                result.Append("<selection id=\"");
                result.Append(timePresets[i].Item1);
                result.Append("\" content=\"");
                result.Append(timePresets[i].Item2);
                result.Append("\"/>");
            }
            result.Append("</input>"
                    + "<input id=\"content\" type=\"text\"/>"
                    + "<action activationType=\"background\" arguments=\"priority1\" content=\"Priority 1\"/>"
                    + "<action activationType=\"background\" arguments=\"priority2\" content=\"Priority 2\"/>"
                    + "<action activationType=\"background\" arguments=\"priority3\" content=\"Priority 3\"/>"
                    + "<action activationType=\"background\" arguments=\"priority4\" content=\"Priority 4\"/>"
                    + "</actions>"
                    + "</toast>");

            return result.ToString();
        }
    }
}
