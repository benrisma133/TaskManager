using System.Windows;
using System.Windows.Controls;

namespace TaskManagerUI.Controls.Components;

public partial class ModernDatePicker : UserControl
{
    // ============================
    // DEPENDENCY PROPERTIES
    // ============================
    public static readonly DependencyProperty SelectedDateProperty =
        DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateOnly?),
            typeof(ModernDatePicker),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedDateChanged));

    public DateOnly? SelectedDate
    {
        get => (DateOnly?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModernDatePicker picker)
            picker._SyncFromDate((DateOnly?)e.NewValue);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(ModernDatePicker),
            new PropertyMetadata("Date", OnLabelChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModernDatePicker picker)
            picker.LabelText.Text = e.NewValue?.ToString() ?? string.Empty;
    }

    public static readonly DependencyProperty YearFromProperty =
        DependencyProperty.Register(
            nameof(YearFrom),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(2020, OnYearRangeChanged));

    public int YearFrom
    {
        get => (int)GetValue(YearFromProperty);
        set => SetValue(YearFromProperty, value);
    }

    public static readonly DependencyProperty YearToProperty =
        DependencyProperty.Register(
            nameof(YearTo),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(2036, OnYearRangeChanged));

    public int YearTo
    {
        get => (int)GetValue(YearToProperty);
        set => SetValue(YearToProperty, value);
    }

    private static void OnYearRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModernDatePicker picker)
            picker._LoadYears();
    }

    // ============================
    // MONTH FROM/TO DEPENDENCY PROPERTIES
    // ============================
    public static readonly DependencyProperty MonthFromProperty =
        DependencyProperty.Register(
            nameof(MonthFrom),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(1, OnMonthRangeChanged));

    public int MonthFrom
    {
        get => (int)GetValue(MonthFromProperty);
        set => SetValue(MonthFromProperty, value);
    }

    public static readonly DependencyProperty MonthToProperty =
        DependencyProperty.Register(
            nameof(MonthTo),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(12, OnMonthRangeChanged));

    public int MonthTo
    {
        get => (int)GetValue(MonthToProperty);
        set => SetValue(MonthToProperty, value);
    }

    private static void OnMonthRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModernDatePicker picker)
            picker._LoadMonths();
    }

    // ============================
    // DAY FROM/TO DEPENDENCY PROPERTIES
    // ============================
    public static readonly DependencyProperty DayFromProperty =
        DependencyProperty.Register(
            nameof(DayFrom),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(1));

    public int DayFrom
    {
        get => (int)GetValue(DayFromProperty);
        set => SetValue(DayFromProperty, value);
    }

    public static readonly DependencyProperty DayToProperty =
        DependencyProperty.Register(
            nameof(DayTo),
            typeof(int),
            typeof(ModernDatePicker),
            new PropertyMetadata(31));

    public int DayTo
    {
        get => (int)GetValue(DayToProperty);
        set => SetValue(DayToProperty, value);
    }

    public static readonly DependencyProperty AllowPastDatesProperty =
    DependencyProperty.Register(
        nameof(AllowPastDates),
        typeof(bool),
        typeof(ModernDatePicker),
        new PropertyMetadata(false));

    public bool AllowPastDates
    {
        get => (bool)GetValue(AllowPastDatesProperty);
        set => SetValue(AllowPastDatesProperty, value);
    }

    // ============================
    // FIELDS
    // ============================
    private bool _isSyncing = false;
    public bool IsValid { get; private set; } = true;

    private static readonly string[] MonthNames =
    {
        "01", "02", "03", "04", "05", "06",
        "07", "08", "09", "10", "11", "12"
    };

    // ============================
    // CONSTRUCTOR
    // ============================
    public ModernDatePicker()
    {
        InitializeComponent();
        LabelText.Text = Label;
        Loaded += (s, e) =>
        {
            _LoadYears();
            _LoadMonths();

            // default to today
            var today = DateOnly.FromDateTime(DateTime.Today);
            _isSyncing = true;
            _SelectByTag(YearCombo, today.Year);
            _SelectByTag(MonthCombo, today.Month);
            _isSyncing = false;
            _LoadDays();
            _SelectByTag(DayCombo, today.Day);
            _TryBuildDate();
        };
    }

    // ============================
    // POPULATE COMBOS
    // ============================
    private void _LoadYears()
    {
        int? selectedYear = _GetSelectedYear();
        YearCombo.Items.Clear();

        // placeholder
        YearCombo.Items.Add(_MakeItem("YYYY", null));

        int currentYear = DateTime.Today.Year;
        int from = YearFrom > 0 ? YearFrom : currentYear;
        int to = YearTo > 0 ? YearTo : currentYear + 10;

        for (int y = from; y <= to; y++)
            YearCombo.Items.Add(_MakeItem(y.ToString(), y));

        // restore selection
        if (selectedYear.HasValue)
            _SelectByTag(YearCombo, selectedYear.Value);
        else
            YearCombo.SelectedIndex = 0;
    }

    private void _LoadMonths()
    {
        int? selectedMonth = _GetSelectedMonth();
        MonthCombo.Items.Clear();

        MonthCombo.Items.Add(_MakeItem("MM", null));

        int from = Math.Max(1, MonthFrom);
        int to = Math.Min(12, MonthTo);

        for (int m = from; m <= to; m++)
            MonthCombo.Items.Add(_MakeItem(m.ToString("D2"), m));

        if (selectedMonth.HasValue)
            _SelectByTag(MonthCombo, selectedMonth.Value);
        else
            MonthCombo.SelectedIndex = 0;
    }

    private void _LoadDays()
    {
        int? selectedDay = _GetSelectedDay();
        DayCombo.Items.Clear();

        DayCombo.Items.Add(_MakeItem("DD", null));

        int? year = _GetSelectedYear();
        int? month = _GetSelectedMonth();

        int maxDays = (year.HasValue && month.HasValue)
            ? DateTime.DaysInMonth(year.Value, month.Value)
            : 31;

        int from = Math.Max(1, DayFrom);
        int to = Math.Min(maxDays, DayTo > 0 ? DayTo : maxDays);

        // if today is selected year/month — start from today's day
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (!AllowPastDates &&
                year.HasValue && month.HasValue &&
                year.Value == today.Year &&
                month.Value == today.Month)
        {
            from = Math.Max(from, today.Day);
        }

        for (int d = from; d <= to; d++)
            DayCombo.Items.Add(_MakeItem(d.ToString("D2"), d));

        if (selectedDay.HasValue)
        {
            int clamped = Math.Clamp(selectedDay.Value, from, to);
            _SelectByTag(DayCombo, clamped);
        }
        else
        {
            DayCombo.SelectedIndex = 0;
        }
    }

    private void _AddPlaceholderItems()
    {
        // days placeholder loaded separately
        _LoadDays();
    }

    // ============================
    // COMBO ITEM FACTORY
    // ============================
    private ComboBoxItem _MakeItem(string display, object? tag)
    {
        var item = new ComboBoxItem
        {
            Content = display,
            Tag = tag
        };

        if (TryFindResource("ModernComboBoxItem") is Style style)
            item.Style = style;

        return item;
    }

    private void _SelectByTag(ComboBox combo, object tag)
    {
        foreach (ComboBoxItem item in combo.Items)
        {
            if (item.Tag?.ToString() == tag.ToString())
            {
                combo.SelectedItem = item;
                return;
            }
        }
        combo.SelectedIndex = 0;
    }

    // ============================
    // GETTERS
    // ============================
    private int? _GetSelectedYear()
    {
        if (YearCombo.SelectedItem is ComboBoxItem y && y.Tag is int yi) return yi;
        return null;
    }

    private int? _GetSelectedMonth()
    {
        if (MonthCombo.SelectedItem is ComboBoxItem m && m.Tag is int mi) return mi;
        return null;
    }

    private int? _GetSelectedDay()
    {
        if (DayCombo.SelectedItem is ComboBoxItem d && d.Tag is int di) return di;
        return null;
    }

    // ============================
    // SYNC FROM DATE → UI
    // ============================
    private void _SyncFromDate(DateOnly? date)
    {
        _isSyncing = true;

        if (date.HasValue)
        {
            _SelectByTag(YearCombo, date.Value.Year);
            _SelectByTag(MonthCombo, date.Value.Month);
            _LoadDays();
            _SelectByTag(DayCombo, date.Value.Day);
        }
        else
        {
            YearCombo.SelectedIndex = 0;
            MonthCombo.SelectedIndex = 0;
            _LoadDays();
            DayCombo.SelectedIndex = 0;
        }

        _isSyncing = false;
    }

    // ============================
    // SYNC FROM UI → DATE
    // ============================
    private void _TryBuildDate()
    {
        if (_isSyncing) return;

        int? year = _GetSelectedYear();
        int? month = _GetSelectedMonth();
        int? day = _GetSelectedDay();

        if (!year.HasValue || !month.HasValue || !day.HasValue)
        {
            SelectedDate = null;
            _SetValid();
            return;
        }

        try
        {
            SelectedDate = new DateOnly(year.Value, month.Value, day.Value);
            _SetValid();
        }
        catch
        {
            _SetInvalid("Invalid date.");
        }
    }

    // ============================
    // VALIDATION
    // ============================
    private void _SetValid()
    {
        IsValid = true;
        ValidationMessage.Visibility = Visibility.Collapsed;
        ValidationMessage.Text = string.Empty;
    }

    private void _SetInvalid(string message)
    {
        IsValid = false;
        ValidationMessage.Text = message;
        ValidationMessage.Visibility = Visibility.Visible;
    }

    public void Clear()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _isSyncing = true;
        _SelectByTag(YearCombo, today.Year);
        _SelectByTag(MonthCombo, today.Month);
        _isSyncing = false;
        _LoadDays();
        _SelectByTag(DayCombo, today.Day);
        _TryBuildDate();
        _SetValid();
    }

    // ============================
    // SELECTION CHANGED
    // ============================
    private void YearCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing) return;
        _LoadDays(); // recalc days for leap year
        _TryBuildDate();
    }

    private void MonthCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing) return;
        _LoadDays(); // recalc days for selected month
        _TryBuildDate();
    }

    private void DayCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncing) return;
        _TryBuildDate();
    }
}