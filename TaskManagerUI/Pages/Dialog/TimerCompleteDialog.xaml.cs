using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TaskManagerUI.Pages.Dialog
{
    public partial class TimerCompleteDialog : Window
    {
        // ============================
        // PROPERTIES
        // ============================
        public enum enDialogResult { MarkDone, AddTime, Cancel }
        public enDialogResult Result { get; private set; } = enDialogResult.Cancel;
        public int ExtraMinutes { get; private set; } = 0;

        // ============================
        // CONSTRUCTOR
        // ============================
        public TimerCompleteDialog(string taskTitle)
        {
            InitializeComponent();
            TaskTitleText.Text = taskTitle;
        }

        // ============================
        // RADIO — select option by clicking border
        // ============================
        private void MarkDoneBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MarkDoneRadio.IsChecked = true;
            _UpdateBorderStyles();
            ExtraMinutesPanel.Visibility = Visibility.Collapsed;
            HideError();
        }

        private void AddTimeBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AddTimeRadio.IsChecked = true;
            _UpdateBorderStyles();
            ExtraMinutesPanel.Visibility = Visibility.Visible;
            HideError();
        }

        private void Radio_Click(object sender, RoutedEventArgs e)
        {
            _UpdateBorderStyles();

            ExtraMinutesPanel.Visibility = AddTimeRadio.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            HideError();
        }

        // ============================
        // UPDATE BORDER STYLES
        // ============================
        private void _UpdateBorderStyles()
        {
            MarkDoneBorder.BorderBrush = MarkDoneRadio.IsChecked == true
                ? TryFindResource("AccentBrush") as Brush
                : TryFindResource("BorderDefaultBrush") as Brush;

            AddTimeBorder.BorderBrush = AddTimeRadio.IsChecked == true
                ? TryFindResource("AccentBrush") as Brush
                : TryFindResource("BorderDefaultBrush") as Brush;
        }

        // ============================
        // SAVE
        // ============================
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (AddTimeRadio.IsChecked == true)
            {
                // validate input
                string raw = ExtraMinutesInput.Text.Trim();

                if (string.IsNullOrEmpty(raw))
                {
                    ShowError("Please enter the number of extra minutes.");
                    ExtraMinutesInput.Focus();
                    return;
                }

                if (!int.TryParse(raw, out int minutes) || minutes <= 0)
                {
                    ShowError("Please enter a valid number greater than 0.");
                    ExtraMinutesInput.Focus();
                    return;
                }

                ExtraMinutes = minutes;
                Result = enDialogResult.AddTime;
            }
            else
            {
                Result = enDialogResult.MarkDone;
            }

            Close();
        }

        // ============================
        // CANCEL
        // ============================
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = enDialogResult.Cancel;
            Close();
        }

        // ============================
        // ERROR HELPERS
        // ============================
        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBox.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBox.Visibility = Visibility.Collapsed;
        }
    }
}