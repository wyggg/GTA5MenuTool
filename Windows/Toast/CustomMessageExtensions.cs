using System.Windows.Media;
using CustomNotificationsExample.CustomMessage;
using ToastNotifications;
using ToastNotifications.Core;

namespace GTA5MenuTools.Windows.Toast
{
    public static class CustomMessageExtensions
    {
        public static void ShowCustomMessage(this Notifier notifier,
            string title,
            string message,
            double titleFont = 14,
            double messageFont = 12,
            string backgroundColor = "#B4EEB4",
            MessageOptions messageOptions = null)
        {
            notifier.Notify(() => new CustomNotification(title, message, titleFont, messageFont, backgroundColor, messageOptions));
        }
    }
}
