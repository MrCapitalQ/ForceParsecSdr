using Microsoft.UI.Xaml;

namespace MrCapitalQ.ForceParsecSdr;

public partial class App : Application
{
    public Window? Window { get; protected set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
    }
}
