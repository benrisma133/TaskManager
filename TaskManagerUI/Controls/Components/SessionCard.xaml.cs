using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManagerUI.Models;

// Fixed namespace
namespace TaskManagerUI.Controls.Components
{
    public partial class SessionCard : UserControl
    {
        public SessionCard()
        {
            InitializeComponent();
        }

        public void LoadSession(TimerSession session)
        {
            StartTimeText.Text = session.StartTimeText;

            DurationText.Text = session.DurationText;
            DurationText.Foreground = session.IsRunning
                ? (Brush)App.Current.Resources["SuccessBrush"]
                : (Brush)App.Current.Resources["TextSecondaryBrush"];

            if (!string.IsNullOrEmpty(session.Notes))
            {
                NotesText.Text = session.Notes;
                NotesText.Visibility = Visibility.Visible;
            }
            else
            {
                NotesText.Visibility = Visibility.Collapsed;
            }
        }
    }
}