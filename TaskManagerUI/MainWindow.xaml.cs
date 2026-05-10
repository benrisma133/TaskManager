using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TaskManagerUI.Helpers;
using TaskManagerUI.Pages.Categories;

namespace TaskManagerUI
{
    public partial class MainWindow : Window
    {
        public static event Action<bool>? ThemeChanged;

        private Button? _activeButton;
        private bool _isSidebarOpen = true;
        private bool _isDark = false;

        private const double SidebarOpenWidth = 220;
        private const double SidebarClosedWidth = 64;

        // Map each button to its popup
        private Dictionary<Button, Popup> _tooltipMap = new();

        public MainWindow()
        {
            InitializeComponent();
            _isDark = Properties.Settings.Default.IsDarkTheme;
            if (!_isDark) ApplyTheme(false);
            SetActiveMenu(BtnHome);

            // Register tooltip mapping after InitializeComponent
            BuildTooltipMap();

            // Set icon from PNG with proper handling for .NET 8
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/Assets/TaskFlow_Icon.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                this.Icon = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Icon load error: {ex.Message}");
            }
        }

        // ── Build the button → popup map ──────────────────────────────
        private void BuildTooltipMap()
        {
            _tooltipMap = new Dictionary<Button, Popup>
            {
                { BtnHome,       PopupHome       },
                { BtnProjects,   PopupProjects   },
                { BtnTasks,      PopupTasks      },
                { BtnCategories, PopupCategories },
                { BtnSettings,   PopupSettings   }
            };
        }

        // ── Tooltip show / hide ───────────────────────────────────────
        private void MenuBtn_MouseEnter(object sender,
                                        System.Windows.Input.MouseEventArgs e)
        {
            // Only show tooltip when sidebar is collapsed
            if (_isSidebarOpen) return;

            if (sender is Button btn && _tooltipMap.TryGetValue(btn, out var popup))
                popup.IsOpen = true;
        }

        private void MenuBtn_MouseLeave(object sender,
                                        System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn && _tooltipMap.TryGetValue(btn, out var popup))
                popup.IsOpen = false;
        }

        // Close all popups (safety helper)
        private void CloseAllPopups()
        {
            foreach (var popup in _tooltipMap.Values)
                popup.IsOpen = false;
        }

        // ── Title bar drag ────────────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender,
                                                   System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) ToggleMaximize();
            else DragMove();
        }

        // ── Window controls ───────────────────────────────────────────
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
            => ToggleMaximize();

        private void BtnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        private void ToggleMaximize()
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        // ── Sidebar toggle ────────────────────────────────────────────
        private void HamburgerBtn_Click(object sender, RoutedEventArgs e)
            => ToggleSidebar();

        private void ToggleSidebar()
        {
            _isSidebarOpen = !_isSidebarOpen;
            double targetWidth = _isSidebarOpen ? SidebarOpenWidth : SidebarClosedWidth;

            // Close all popups when toggling
            CloseAllPopups();

            // ✅ Disable effects before animation
            SetPageRenderingMode(true);

            var anim = new GridLengthAnimation
            {
                From = SidebarColumn.Width,
                To = new GridLength(targetWidth),
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            anim.Completed += (s, e) =>
            {
                // ✅ Re-enable effects after animation completes
                SetPageRenderingMode(false);
            };

            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, anim);

            // Show/hide text labels
            var vis = _isSidebarOpen ? Visibility.Visible : Visibility.Collapsed;
            HomeText.Visibility = vis;
            ProjectsText.Visibility = vis;
            TasksText.Visibility = vis;
            CategoriesText.Visibility = vis;
            SettingsText.Visibility = vis;
            ProfileStack.Visibility = vis;
        }

        private void SetPageRenderingMode(bool animating)
        {
            if (PageContent.Content is UIElement page)
            {
                if (animating)
                {
                    // ✅ Freeze rendering to a bitmap during animation
                    page.CacheMode = new BitmapCache { EnableClearType = false, SnapsToDevicePixels = false };
                }
                else
                {
                    // ✅ Restore normal rendering after animation
                    page.CacheMode = null;
                }
            }
        }

        // ── Active menu ───────────────────────────────────────────────
        private void SetActiveMenu(Button btn)
        {
            if (_activeButton != null)
                _activeButton.ClearValue(TagProperty);
            _activeButton = btn;
            _activeButton.Tag = "Active";
        }

        // ── Navigation ────────────────────────────────────────────────
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(BtnHome);
            PageContent.Content = null;
        }

        private void BtnProjects_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(BtnProjects);
            PageContent.Content = null;
        }

        private void BtnTasks_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(BtnTasks);
            PageContent.Content = null;
        }

        private CategoriesPage _categoryPage = new CategoriesPage();

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(BtnCategories);
            PageContent.Content = _categoryPage;
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SetActiveMenu(BtnSettings);
            PageContent.Content = null;
        }

        // ── Theme ─────────────────────────────────────────────────────
        private void ApplyTheme(bool isDark)
        {
            _isDark = isDark;
            Properties.Settings.Default.IsDarkTheme = isDark;
            Properties.Settings.Default.Save();

            var source = isDark
                ? new Uri("/Helpers/Colors.xaml", UriKind.Relative)
                : new Uri("/Helpers/ColorsLight.xaml", UriKind.Relative);

            var newDict = new ResourceDictionary { Source = source };
            foreach (var key in newDict.Keys)
                App.Current.Resources[key] = newDict[key];

            ThemeChanged?.Invoke(isDark);
        }
    }
}