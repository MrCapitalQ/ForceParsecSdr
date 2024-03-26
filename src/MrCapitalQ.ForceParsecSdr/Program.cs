using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using MrCapitalQ.ForceParsecSdr;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class Program
{
    private const string AppKey = "943116d4-f8a9-41d2-bc03-97049f87886a";

    [STAThread]
    private static async Task Main()
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();

        var keyInstance = AppInstance.FindOrRegisterForKey(AppKey);
        if (!keyInstance.IsCurrent)
        {
            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            await keyInstance.RedirectActivationToAsync(activationArgs);
            return;
        }

        Application.Start((p) =>
        {
            var context = new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            _ = new App();
        });
    }
}