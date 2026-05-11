using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace TaskManagerUI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        // 1. Declare the event — anyone can subscribe to this
        public event Action<bool>? ThemeToggled;
        private bool _isDark;

        public SettingsPage()
        {
            InitializeComponent();
        }

        private bool _isInitializing = true;

        public void SetThemeState(bool isDark)
        {
            _isDark = isDark;
            _isInitializing = true;
            ThemeToggle.IsChecked = isDark;
            _isInitializing = false;
            SwitchTitles(isDark);
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ThemeToggled?.Invoke(true);
            SwitchTitles(true);
            PlayToggleSound();
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            ThemeToggled?.Invoke(false);
            SwitchTitles(false);
            PlayToggleSound();
        }

        void SwitchTitles(bool isDark)
        {
            DarkModeTitle.Text = isDark ? "Dark Mode" : "Light Mode";
            DarkModeDescription.Text = isDark ? "Switch to the (Light Mode)" : "Switch to the (Dark Mode)";
        }

        private void PlayToggleSound()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/TaskManagerUI;component/Assets/Sounds/toggle.wav");
                var info = Application.GetResourceStream(uri);
                var player = new SoundPlayer(info.Stream);
                player.Play();
            }
            catch { }
        }
    }
}
