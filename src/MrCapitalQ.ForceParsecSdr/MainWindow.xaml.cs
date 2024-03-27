using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.ApplicationModel;

namespace MrCapitalQ.ForceParsecSdr;

public sealed partial class MainWindow : Window
{
    private const string StartupTaskId = "6896544d-7a11-4322-a744-571d655f32f7";

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        AppWindow.Resize(new(600, 400));

        AppWindow.Closing += AppWindow_Closing;

        var parsecConnectionTracker = new ParsecConnectionTracker();
        parsecConnectionTracker.ConnectionStatusChanged += ParsecConnectionTracker_ConnectionStatusChanged;

        UpdateStartupState();
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        sender.Hide();
        args.Cancel = true;
    }

    private void ParsecConnectionTracker_ConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (HdrHelper.IsHdrAvailable())
            HdrHelper.SetHdrState(!e.IsConnected);
    }

    private async void UpdateStartupState()
    {
        StartupWarning.Visibility = Visibility.Collapsed;
        var isStartupEnabled = false;
        var isStartupSettable = true;

        var startupTask = await StartupTask.GetAsync(StartupTaskId);
        switch (startupTask.State)
        {
            case StartupTaskState.DisabledByUser:
                StartupWarning.Visibility = Visibility.Visible;
                StartupWarning.Message = "You have disabled this app's ability to run at startup. Re-enable this in the Startup tab in Task Manager.";
                isStartupSettable = false;
                break;
            case StartupTaskState.DisabledByPolicy:
                StartupWarning.Visibility = Visibility.Visible;
                StartupWarning.Message = "Startup is disabled by group policy or not supported on this device";
                isStartupSettable = false;
                break;
            case StartupTaskState.Disabled:
                break;
            case StartupTaskState.Enabled:
                isStartupEnabled = true;
                break;
        }

        StartupSwitch.IsOn = isStartupEnabled;
        StartupSwitch.IsEnabled = isStartupSettable;
    }

    private async void StartupSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        StartupSwitch.IsEnabled = false;
        var startupTask = await StartupTask.GetAsync(StartupTaskId);

        if (StartupSwitch.IsOn)
            await startupTask.RequestEnableAsync();
        else
            startupTask.Disable();

        UpdateStartupState();
    }
}
