using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace MrCapitalQ.ForceParsecSdr;

internal partial class ParsecConnectionTracker
{
    public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

    private readonly HashSet<string> _connectedUsers = [];

    public ParsecConnectionTracker()
    {
        var listener = UserNotificationListener.Current;
        listener.NotificationChanged += Listener_NotificationChanged;
    }

    public bool IsConnected => _connectedUsers.Count != 0;

    [GeneratedRegex(@"(.+) (disconnected|connected)\.")]
    private static partial Regex NotificationTitleRegex();

    private void OnConnectionsChange()
    {
        var raiseEvent = ConnectionStatusChanged;
        raiseEvent?.Invoke(this, new(IsConnected));
    }

    private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
    {
        if (args.ChangeKind != UserNotificationChangedKind.Added)
            return;

        var notification = sender.GetNotification(args.UserNotificationId);
        if (notification.AppInfo.DisplayInfo.DisplayName != "Parsec")
            return;

        var toastBinding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
        if (toastBinding is null)
            return;

        var textElements = toastBinding.GetTextElements();
        var titleText = textElements.Count > 0
            ? textElements[0].Text
            : null;

        if (titleText is null)
            return;

        if (NotificationTitleRegex().Match(titleText) is Match match && match.Success)
        {
            var originalState = IsConnected;
            switch (match.Groups[2].Value)
            {
                case "connected":
                    _connectedUsers.Add(match.Groups[1].Value);
                    break;
                case "disconnected":
                    _connectedUsers.Remove(match.Groups[1].Value);
                    break;
                default:
                    break;
            }

            if (originalState != IsConnected)
                OnConnectionsChange();
        }
    }
}

internal class ConnectionStatusChangedEventArgs : EventArgs
{
    public ConnectionStatusChangedEventArgs(bool isConnected) => IsConnected = isConnected;

    public bool IsConnected { get; }
}
