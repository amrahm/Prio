using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace Infrastructure.SharedResources {
    public static class NotificationBubbler {
        /// <summary> Bubble up changes from within the object </summary>
        /// <param name="obj"> object to be set </param>
        /// <param name="value"> new value for object </param>
        /// <param name="thisChanged"> Lets you specify what OnPropertyChanged method should be called </param>
        public static void BubbleSetter<T>(ref T obj, T value, PropertyChangedEventHandler thisChanged)
            where T : class, INotifyPropertyChanged {
            if(obj != value) {
                // Clean-up old event handler:
                if(obj != null) obj.PropertyChanged -= thisChanged;

                obj = value;

                if(obj != null) obj.PropertyChanged += thisChanged;
            }
        }
    }


    public static class WindowHelpers {
        public static Screen CurrentScreen(this Window window) {
            return Screen.FromPoint(new System.Drawing.Point((int) window.Left, (int) window.Top));
        }
    }
}