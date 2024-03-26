using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace MrCapitalQ.ForceParsecSdr;

public partial class App : Application
{
    public Window? Window { get; protected set; }

    public App()
    {
        InitializeComponent();
        AppInstance.GetCurrent().Activated += App_Activated;
    }

    private void App_Activated(object? sender, AppActivationArguments e)
    {
        if (e.Kind == ExtendedActivationKind.Launch)
            Window?.DispatcherQueue.TryEnqueue(() => Window.Activate());
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();

        if (AppInstance.GetCurrent().GetActivatedEventArgs().Kind != ExtendedActivationKind.StartupTask)
            Window.Activate();
    }
}
