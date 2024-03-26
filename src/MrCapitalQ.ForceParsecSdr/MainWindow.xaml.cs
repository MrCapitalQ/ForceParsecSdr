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
        var isStartupEnabled = false;
        var isStartupEnablable = true;

        var startupTask = await StartupTask.GetAsync(StartupTaskId);
        switch (startupTask.State)
        {
            case StartupTaskState.DisabledByUser:
                InfoTextBlock.Text = "You have disabled this app's ability to run at startup. Re-enable this in the Startup tab in Task Manager.";
                isStartupEnablable = false;
                break;
            case StartupTaskState.DisabledByPolicy:
                InfoTextBlock.Text = "Startup is disabled by group policy or not supported on this device";
                isStartupEnablable = false;
                break;
            case StartupTaskState.Disabled:
                InfoTextBlock.Text = "The app will not automatically run at startup.";
                break;
            case StartupTaskState.Enabled:
                InfoTextBlock.Text = "The app will automatically run at startup.";
                isStartupEnabled = true;
                break;
        }

        StartupSwitch.IsOn = isStartupEnabled;
        StartupSwitch.IsEnabled = isStartupEnablable;
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
