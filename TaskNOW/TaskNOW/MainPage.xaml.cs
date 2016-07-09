using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TaskNOW
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ComboBox[] ProjectsBoxes;
        private ComboBox[] DatesBoxes;
        private ComboBox[] TimesBoxes;

        bool PresetBoxesBeingSet = false;

        private string token = null;
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 250, Height = 400 });
            ProjectsBoxes = new ComboBox[5];
            DatesBoxes = new ComboBox[5];
            TimesBoxes = new ComboBox[5];
            for (int i = 0; i < 5; i++)
            {
                ProjectsBoxes[i] = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(4) };
                ProjectsPanel.Children.Add(ProjectsBoxes[i]);
                DatesBoxes[i] = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(4) };
                DatesPanel.Children.Add(DatesBoxes[i]);
                TimesBoxes[i] = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(4) };
                TimesPanel.Children.Add(TimesBoxes[i]);
            }
        }

        #region Methods
        private async Task EnsureBackgroundTask()
        {
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            var versionString = $"{version.Major}.{version.Minor}.{version.Revision}.{version.Build}";
            if (versionString != SettingsManager.GetSetting<string>("CurrentVersion", false, ""))
            {
                BackgroundExecutionManager.RemoveAccess();
                SettingsManager.SetSetting<string>("CurrentVersion", false, versionString);
            }
            var status = await BackgroundExecutionManager.RequestAccessAsync();
            if (status == BackgroundAccessStatus.Denied)
            {
                await (new MessageDialog("Could not register background task. Please restart the app and allow it to run in the background.")).ShowAsync();
            }

            var taskRegistered = false;
            var taskName = "TaskNOWBackgroundTask";
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();
                builder.Name = taskName;
                builder.TaskEntryPoint = "TaskNOWBackground.TaskNOWBackgroundTask";
                builder.SetTrigger(new ToastNotificationActionTrigger());
                var registration = builder.Register();
                
            }
        }

        private void SetPresetBoxes()
        {
            PresetBoxesBeingSet = true;
            for (int i = 0; i < 5; i++)
            {
                ProjectsBoxes[i].SelectedItem = ProjectsBoxes[i].Items.FirstOrDefault(item => (int)((ComboBoxItem)item).Tag == SettingsManager.GetSetting<int>("ProjectPreset" + i, true, -1));
                DatesBoxes[i].SelectedItem = DatesBoxes[i].Items.FirstOrDefault(item => (int)((ComboBoxItem)item).Tag == SettingsManager.GetSetting<int>("DatePreset" + i, true, -1));
                TimesBoxes[i].SelectedItem = TimesBoxes[i].Items.FirstOrDefault(item => (int)((ComboBoxItem)item).Tag == SettingsManager.GetSetting<int>("TimePreset" + i, true, -1));
            }
            PresetBoxesBeingSet = false;
        }

        private async Task RefreshControlsShown()
        {
            MainStackPanel.Visibility = Visibility.Collapsed;
            LoadingIndicator.Visibility = Visibility.Visible;
            LoadingIndicator.IsActive = true;
            bool loggedIn = ((token = SettingsManager.GetSetting<string>("Token", true)) != null);
            //if (loggedIn)
            //    loggedIn = ((token = await Authentication.GetAccessToken(authCode)) != null);
            if (loggedIn)
            {
                var result = await HttpRequests.HttpPost("https://todoist.com/API/v7/sync", "token", token, "sync_token", "*", "resource_types", "[\"projects\"]");
                if (!result.IsSuccessStatusCode)
                    loggedIn = false;
                else
                {
                    string stringResult = await result.Content.ReadAsStringAsync();
                    ResponseBlock.Text = stringResult.Replace("{", "{\n").Replace("}", "\n}");
                    var projects = Parsing.ParseProjects(stringResult).Where(project => !project.Deleted);
                    PresetManager.SetProjectPresets(projects.ToList());

                    PresetBoxesBeingSet = true;

                    for (int i = 0; i < 5; i++)
                    {
                        ProjectsBoxes[i].Items.Clear();
                        ProjectsBoxes[i].Items.Add(new ComboBoxItem() { Content = "(Unused)", Foreground = new SolidColorBrush(Colors.DarkGray), Tag = (int)(-1) });
                        foreach (var project in projects)
                        {
                            ProjectsBoxes[i].Items.Add(new ComboBoxItem() { Content = project.Name, Foreground = new SolidColorBrush(project.Archived ? Colors.DarkGray : Colors.Black), Tag = project.Id });
                        }

                        DatesBoxes[i].Items.Clear();
                        DatesBoxes[i].Items.Add(new ComboBoxItem() { Content = "(Unused)", Foreground = new SolidColorBrush(Colors.DarkGray), Tag = (int)(-1) });
                        foreach (var preset in PresetManager.GetDatePresets())
                        {
                            DatesBoxes[i].Items.Add(new ComboBoxItem() { Content = preset.Item2, Tag = preset.Item1 });
                        }

                        TimesBoxes[i].Items.Clear();
                        TimesBoxes[i].Items.Add(new ComboBoxItem() { Content = "(Unused)", Foreground = new SolidColorBrush(Colors.DarkGray), Tag = (int)(-1) });
                        foreach (var preset in PresetManager.GetTimePresets())
                        {
                            TimesBoxes[i].Items.Add(new ComboBoxItem() { Content = preset.Item2, Tag = preset.Item1 });
                        }
                    }

                    SetPresetBoxes();
                }
            }
            LoadingIndicator.IsActive = false;
            LoadingIndicator.Visibility = Visibility.Collapsed;
            MainStackPanel.Visibility = Visibility.Visible;
            LoginButton.Visibility = loggedIn ? Visibility.Collapsed : Visibility.Visible;
            LogoutButton.Visibility = loggedIn ? Visibility.Visible : Visibility.Collapsed;
        }

        private async Task Login()
        {
            string authCode = await HttpRequests.Authenticate();
            token = await HttpRequests.GetAccessToken(authCode);
            SettingsManager.SetSetting<string>("Token", true, token);
            await RefreshControlsShown();
        }

        private async Task Logout()
        {
            SettingsManager.SetSetting<string>("Token", true, null);
            await RefreshControlsShown();
        }
        #endregion

        #region Event Handlers
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            await Logout();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string debugMessage = SettingsManager.GetSetting<string>("DebugMessage", false, null);
            if (debugMessage != null)
            {
                SettingsManager.SetSetting<string>("DebugMessage", false, null);
                (new MessageDialog(debugMessage)).ShowAsync().ToString();
            }
            for (int i = 0; i < 5; i++)
            {
                ProjectsBoxes[i].SelectionChanged += PresetBox_SelectionChanged;
                DatesBoxes[i].SelectionChanged += PresetBox_SelectionChanged;
                TimesBoxes[i].SelectionChanged += PresetBox_SelectionChanged;
            }
            await RefreshControlsShown();
            await EnsureBackgroundTask();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                ProjectsBoxes[i].SelectionChanged -= PresetBox_SelectionChanged;
                DatesBoxes[i].SelectionChanged -= PresetBox_SelectionChanged;
                TimesBoxes[i].SelectionChanged -= PresetBox_SelectionChanged;
            }
        }

        private void PresetBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool manualDate = false;
            bool manualTime = false;
            for (int i = 0; i < 5; i++)
            {
                if (DatesBoxes[i].SelectedItem != null && (int)((ComboBoxItem)DatesBoxes[i].SelectedItem).Tag != -1)
                {
                    manualDate |= (PresetManager.GetDatePresets().First(item => item.Item1 == (int)((ComboBoxItem)DatesBoxes[i].SelectedItem).Tag).Item3 == null);
                }
                if (TimesBoxes[i].SelectedItem != null && (int)((ComboBoxItem)TimesBoxes[i].SelectedItem).Tag != -1)
                {
                    manualTime |= (PresetManager.GetTimePresets().First(item => item.Item1 == (int)((ComboBoxItem)TimesBoxes[i].SelectedItem).Tag).Item3 == null);
                }
                if (!PresetBoxesBeingSet)
                {
                    if (ProjectsBoxes[i].SelectedItem == null)
                        SettingsManager.SetSetting<int>("ProjectPreset" + i, true, -1);
                    else
                        SettingsManager.SetSetting<int>("ProjectPreset" + i, true, (int)((ComboBoxItem)ProjectsBoxes[i].SelectedItem).Tag);

                    if (DatesBoxes[i].SelectedItem == null)
                        SettingsManager.SetSetting<int>("DatePreset" + i, true, -1);
                    else
                        SettingsManager.SetSetting<int>("DatePreset" + i, true, (int)((ComboBoxItem)DatesBoxes[i].SelectedItem).Tag);

                    if (TimesBoxes[i].SelectedItem == null)
                        SettingsManager.SetSetting<int>("TimePreset" + i, true, -1);
                    else
                        SettingsManager.SetSetting<int>("TimePreset" + i, true, (int)((ComboBoxItem)TimesBoxes[i].SelectedItem).Tag);
                }
                ManualEntryInstruction.Visibility = (manualDate || manualTime) ? Visibility.Visible : Visibility.Collapsed;
                ManualDateTimeExample.Visibility = (manualDate && manualTime) ? Visibility.Visible : Visibility.Collapsed;
                ManualDateExample.Visibility = (manualDate && !manualTime) ? Visibility.Visible : Visibility.Collapsed;
                ManualTimeExample.Visibility = (!manualDate && manualTime) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToastManager.ShowToast();
        }
    }
}
