using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaskManagerUI.Controls.Components;

public partial class ColorPicker : UserControl
{
    // ============================
    // COLOR ITEM MODEL
    // ============================
    public class ColorItem : INotifyPropertyChanged
    {
        public string Hex { get; set; } = null!;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    // ============================
    // DEPENDENCY PROPERTY
    // ============================
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(string),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(
                "#6366F1",
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged));

    public string SelectedColor
    {
        get => (string)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPicker picker)
            picker._SyncSelection((string)e.NewValue);
    }

    // ============================
    // FIELDS
    // ============================
    private readonly ObservableCollection<ColorItem> _colors = new();

    // ============================
    // DEFAULT COLORS
    // ============================
    private static readonly List<string> DefaultColors = new()
    {
        // Blues
        "#3B82F6", "#2563EB", "#1D4ED8", "#60A5FA",
        // Purples
        "#8B5CF6", "#7C3AED", "#6D28D9", "#A78BFA",
        // Pinks
        "#EC4899", "#DB2777", "#BE185D", "#F472B6",
        // Reds
        "#EF4444", "#DC2626", "#B91C1C", "#F87171",
        // Oranges
        "#F97316", "#EA580C", "#C2410C", "#FB923C",
        // Yellows
        "#F59E0B", "#D97706", "#B45309", "#FCD34D",
        // Greens
        "#10B981", "#059669", "#047857", "#34D399",
        // Teals
        "#06B6D4", "#0891B2", "#0E7490", "#67E8F9",
    };

    // ============================
    // CONSTRUCTOR
    // ============================
    public ColorPicker()
    {
        InitializeComponent();

        _LoadColors();
        ColorGrid.ItemsSource = _colors;

        // select first by default
        _SyncSelection(SelectedColor);
    }

    // ============================
    // LOAD COLORS
    // ============================
    private void _LoadColors()
    {
        foreach (var hex in DefaultColors)
            _colors.Add(new ColorItem { Hex = hex });
    }

    // ============================
    // SYNC SELECTION
    // ============================
    private void _SyncSelection(string hex)
    {
        foreach (var item in _colors)
            item.IsSelected = item.Hex.Equals(hex, StringComparison.OrdinalIgnoreCase);

        _UpdatePreview(hex);
    }

    // ============================
    // UPDATE PREVIEW
    // ============================
    private void _UpdatePreview(string hex)
    {
        try
        {
            SelectedPreview.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(hex));
            SelectedHexText.Text = hex.ToUpper();
        }
        catch
        {
            // invalid hex — ignore
        }
    }

    // ============================
    // COLOR CLICK
    // ============================
    private void ColorSwatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string hex)
        {
            SelectedColor = hex;
            _SyncSelection(hex);
        }
    }
}