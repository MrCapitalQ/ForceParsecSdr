using Microsoft.UI.Xaml;
namespace MrCapitalQ.ForceParsecSdr;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var parsecConnectionTracker = new ParsecConnectionTracker();
        parsecConnectionTracker.ConnectionStatusChanged += ParsecConnectionTracker_ConnectionStatusChanged;
    }

    private void ParsecConnectionTracker_ConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
    {
        if (HdrHelper.IsHdrAvailable())
            HdrHelper.SetHdrState(!e.IsConnected);
    }
}
