using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CustomNotificationsExample.CustomMessage;
using ToastNotifications.Core;

namespace GTA5MenuTools.Windows.Toast
{
    public class CustomNotification : NotificationBase, INotifyPropertyChanged
    {
        private CustomDisplayPart _displayPart;

        public override NotificationDisplayPart DisplayPart => _displayPart ?? (_displayPart = new CustomDisplayPart(this));

        public CustomNotification(
            string title,
            string message,
            double titleFont,
            double messageFont,
            string backgroundColor,
            MessageOptions messageOptions) : base(message, messageOptions)
        {
            BackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor));
            BackgroundColor.Opacity = 0.9;
            TitleFont = titleFont;
            MessageFont = messageFont;
            Title = title;
            Message = message;
        }

        #region binding properties
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private double _titleFont;
        public double TitleFont
        {
            get
            {
                return _titleFont;
            }
            set
            {
                _titleFont = value;
                OnPropertyChanged();
            }
        }

        private double _messageFont;
        public double MessageFont
        {
            get
            {
                return _messageFont;
            }
            set
            {
                _messageFont = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush _backgroundColor;
        public SolidColorBrush BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
