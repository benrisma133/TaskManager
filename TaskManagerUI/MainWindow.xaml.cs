using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TaskManagerUI.Helpers;
using TaskManagerUI.Pages.Categories;
using TaskManagerUI.Pages.Projects;
using TaskManagerUI.Pages.Settings;
using TaskManagerUI.Pages.Tasks;

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

        private CategoriesPage _categoryPage;
        private TasksPage _taskPage;
        private ProjectsPage _projectPage;


        public MainWindow()
        {
            InitializeComponent();
            // Load saved theme
            _isDark = Properties.Settings.Default.IsDarkTheme;
            if (!_isDark)
            {
                var newDict = new ResourceDictionary
                {
                    Source = new Uri("/Helpers/ColorsLight.xaml", UriKind.Relative)
                };
                foreach (var key in newDict.Keys)
                    App.Current.Resources[key] = newDict[key];
            }


            //if (!_isDark) ApplyTheme(false);
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

            _categoryPage = new CategoriesPage();
            _taskPage = new TasksPage();
            _projectPage = new ProjectsPage();

            Loaded += (s, e) =>
            {
                if (PageContent.Content is UIElement page)
                {
                    // ✅ Force first render and cache it
                    page.CacheMode = new BitmapCache();
                    page.UpdateLayout(); // Force WPF to render once
                    page.CacheMode = null; // Release cache
                }
            };
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
            //PlaySuccessSound("tap.wav");
        }

        private void PlaySuccessSound(string soundName)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/TaskManagerUI;component/Assets/Sounds/{soundName}");
                var info = Application.GetResourceStream(uri);
                var player = new SoundPlayer(info.Stream);
                player.Play();
            }
            catch { }
        }

        // ── Navigation ────────────────────────────────────────────────
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            if (BtnHome.Tag?.ToString() == "Active") return;
            PlaySuccessSound("tap.wav");
            SetActiveMenu(BtnHome);
            PageTitle.Text = "Home";
            PageContent.Content = null;
        }

        private void BtnProjects_Click(object sender, RoutedEventArgs e)
        {
            if (BtnProjects.Tag?.ToString() == "Active") return;
            PlaySuccessSound("tap.wav");
            SetActiveMenu(BtnProjects);
            PageTitle.Text = "Projects";
            PageContent.Content = _projectPage;
        }

        private void BtnTasks_Click(object sender, RoutedEventArgs e)
        {
            if (BtnTasks.Tag?.ToString() == "Active") return;
            PlaySuccessSound("tap.wav");
            SetActiveMenu(BtnTasks);
            PageTitle.Text = "Tasks";
            PageContent.Content = _taskPage;
        }

        

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            if (BtnCategories.Tag?.ToString() == "Active") return;
            PlaySuccessSound("tap.wav");
            SetActiveMenu(BtnCategories);
            PageTitle.Text = "Categories";
            PageContent.Content = _categoryPage;
        }

        //private bool _isDark;

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (BtnSettings.Tag?.ToString() == "Active") return;
            PlaySuccessSound("tap.wav");
            SetActiveMenu(BtnSettings);
            PageTitle.Text = "Settings";

            var settingsPage = new SettingsPage();

            // 4. Subscribe to the event
            settingsPage.ThemeToggled += OnThemeToggled;

            // 5. Sync toggle with current theme state
            settingsPage.SetThemeState(_isDark);

            PageContent.Content = settingsPage;
        }

        // 6. This runs when SettingsPage fires ThemeToggled
        private void OnThemeToggled(bool isDark)
        {
            _isDark = isDark;
            Properties.Settings.Default.IsDarkTheme = _isDark;
            Properties.Settings.Default.Save();

            var source = _isDark
                ? new Uri("/Helpers/Colors.xaml", UriKind.Relative)
                : new Uri("/Helpers/ColorsLight.xaml", UriKind.Relative);

            var newDict = new ResourceDictionary { Source = source };

            foreach (var key in newDict.Keys)
                App.Current.Resources[key] = newDict[key];

            ThemeChanged?.Invoke(_isDark);
        }
    }
}