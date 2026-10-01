using System.Windows;

namespace TaskManagerUI.Pages.Dialog
{
    public partial class AddExtraTimeDialog : Window
    {
        public bool Confirmed { get; private set; } = false;
        public int ExtraMinutes { get; private set; } = 0;

        public AddExtraTimeDialog(string taskTitle)
        {
            InitializeComponent();
            TaskTitleText.Text = taskTitle;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            HideError();
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
            Confirmed = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBox.Visibility = Visibility.Visible;
        }

        private void HideError() => ErrorBox.Visibility = Visibility.Collapsed;
    }
}