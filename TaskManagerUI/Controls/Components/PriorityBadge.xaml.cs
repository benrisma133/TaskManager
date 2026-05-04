using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskManagerUI.Controls.Components
{
    /// <summary>
    /// Interaction logic for PriorityBadge.xaml
    /// </summary>
    public partial class PriorityBadge : UserControl
    {
        public PriorityBadge()
        {
            InitializeComponent();
        }

        // 1. Register the DependencyProperty
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                "Status",                    // property name
                typeof(string),              // property type
                typeof(PriorityBadge),         // owner class
                new PropertyMetadata("", OnStatusChanged)  // default value + callback
            );

        // 2. CLR wrapper — this is what you use in C# code
        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        // 3. Called automatically when Status value changes
        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var badge = (PriorityBadge)d;
            badge.UpdateBadge((string)e.NewValue);
        }

        private void UpdateBadge(string status)
        {
            BadgeText.Text = status;

            switch (status?.ToLower())
            {
                case "low":
                    BadgeBorder.Background = (Brush)App.Current.Resources["SuccessLightBrush"];
                    BadgeText.Foreground = (Brush)App.Current.Resources["SuccessBrush"];
                    break;
                case "medium":
                    BadgeBorder.Background = (Brush)App.Current.Resources["WarningLightBrush"];
                    BadgeText.Foreground = (Brush)App.Current.Resources["WarningBrush"];
                    break;
                case "high":
                    BadgeBorder.Background = (Brush)App.Current.Resources["PriorityHighLightBrush"];
                    BadgeText.Foreground = (Brush)App.Current.Resources["PriorityHighBrush"];
                    break;
                case "critical":
                    BadgeBorder.Background = (Brush)App.Current.Resources["ErrorLightBrush"];
                    BadgeText.Foreground = (Brush)App.Current.Resources["ErrorBrush"];
                    break;
                default:
                    BadgeBorder.Background = (Brush)App.Current.Resources["CardHoverBackgroundBrush"];
                    BadgeText.Foreground = (Brush)App.Current.Resources["TextSecondaryBrush"];
                    break;
            }
        }
    }
}
