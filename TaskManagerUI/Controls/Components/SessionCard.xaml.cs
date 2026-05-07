using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TaskManagerUI.Controls.Components
{
    public partial class SessionCard : UserControl
    {
        // ── DependencyProperties ──────────────────────────────────────────

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(string), typeof(SessionCard),
                new PropertyMetadata(string.Empty, OnStartTimeChanged));

        public string StartTime
        {
            get => (string)GetValue(StartTimeProperty);
            set => SetValue(StartTimeProperty, value);
        }

        private static void OnStartTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (SessionCard)d;
            if (card.StartTimeText != null)
                card.StartTimeText.Text = e.NewValue as string;
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register(nameof(Duration), typeof(string), typeof(SessionCard),
                new PropertyMetadata(string.Empty, OnDurationChanged));

        public string Duration
        {
            get => (string)GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (SessionCard)d;
            if (card.DurationText != null)
                card.DurationText.Text = e.NewValue as string;
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register(nameof(Notes), typeof(string), typeof(SessionCard),
                new PropertyMetadata(string.Empty, OnNotesChanged));

        public string Notes
        {
            get => (string)GetValue(NotesProperty);
            set => SetValue(NotesProperty, value);
        }

        private static void OnNotesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (SessionCard)d;
            if (card.NotesText == null) return;
            var text = e.NewValue as string;
            card.NotesText.Text = text;
            card.NotesText.Visibility = string.IsNullOrEmpty(text)
                                        ? Visibility.Collapsed
                                        : Visibility.Visible;
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(SessionCard),
                new PropertyMetadata(false, OnIsRunningChanged));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (SessionCard)d;
            if (card.DurationText == null) return;

            if ((bool)e.NewValue)
            {
                // Switch to SuccessBrush
                card.DurationText.Foreground = (Brush)App.Current.Resources["SuccessBrush"];

                // Pulsing opacity animation
                var animation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.3,
                    Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                card.DurationText.BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                // Stop animation and restore color
                card.DurationText.BeginAnimation(OpacityProperty, null);
                card.DurationText.Opacity = 1.0;
                card.DurationText.Foreground = (Brush)App.Current.Resources["TextSecondaryBrush"];
            }
        }

        // ── Constructor ───────────────────────────────────────────────────

        public SessionCard()
        {
            InitializeComponent();
        }
    }
}