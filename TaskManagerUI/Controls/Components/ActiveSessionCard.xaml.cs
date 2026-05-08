using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TaskManagerUI.Controls.Components
{
    public partial class ActiveSessionCard : UserControl
    {
        // ── DependencyProperties ──────────────────────────────────────────

        public static readonly DependencyProperty TaskTitleProperty =
            DependencyProperty.Register(nameof(TaskTitle), typeof(string), typeof(ActiveSessionCard),
                new PropertyMetadata(string.Empty, (d, e) =>
                    ((ActiveSessionCard)d).TaskTitleText.Text = e.NewValue as string));

        public string TaskTitle
        {
            get => (string)GetValue(TaskTitleProperty);
            set => SetValue(TaskTitleProperty, value);
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(nameof(Subtitle), typeof(string), typeof(ActiveSessionCard),
                new PropertyMetadata(string.Empty, (d, e) =>
                    ((ActiveSessionCard)d).SubtitleText.Text = e.NewValue as string));

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty ElapsedTimeProperty =
            DependencyProperty.Register(nameof(ElapsedTime), typeof(string), typeof(ActiveSessionCard),
                new PropertyMetadata("00:00:00", (d, e) =>
                    ((ActiveSessionCard)d).ElapsedTimeText.Text = e.NewValue as string));

        public string ElapsedTime
        {
            get => (string)GetValue(ElapsedTimeProperty);
            set => SetValue(ElapsedTimeProperty, value);
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ActiveSessionCard),
                new PropertyMetadata(0.0, (d, e) =>
                    ((ActiveSessionCard)d).SessionProgress.ProgressWidth = (double)e.NewValue));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        // ─────────────────────────────────────────────────────────────────

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(ActiveSessionCard),
                new PropertyMetadata(true, OnIsRunningChanged));

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var card = (ActiveSessionCard)d;
            bool running = (bool)e.NewValue;

            if (running)
            {
                card.StatusText.Text = "Running";
                card.StatusText.Foreground = (Brush)App.Current.Resources["SuccessBrush"];
                card.RunningDot.Fill = (Brush)App.Current.Resources["SuccessBrush"];
                card.StartPulseAnimation();
            }
            else
            {
                card.StatusText.Text = "Paused";
                card.StatusText.Foreground = (Brush)App.Current.Resources["TextMutedBrush"];
                card.RunningDot.Fill = (Brush)App.Current.Resources["TextMutedBrush"];
                card.StopPulseAnimation();
            }
        }

        // ── Routed Events ─────────────────────────────────────────────────

        public static readonly RoutedEvent PauseClickEvent =
            EventManager.RegisterRoutedEvent(nameof(PauseClick), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ActiveSessionCard));

        public event RoutedEventHandler PauseClick
        {
            add => AddHandler(PauseClickEvent, value);
            remove => RemoveHandler(PauseClickEvent, value);
        }

        public static readonly RoutedEvent StopClickEvent =
            EventManager.RegisterRoutedEvent(nameof(StopClick), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ActiveSessionCard));

        public event RoutedEventHandler StopClick
        {
            add => AddHandler(StopClickEvent, value);
            remove => RemoveHandler(StopClickEvent, value);
        }

        // ── Constructor ───────────────────────────────────────────────────

        public ActiveSessionCard()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PauseBtn.Click += (s, args) => RaiseEvent(new RoutedEventArgs(PauseClickEvent));
            StopBtn.Click += (s, args) => RaiseEvent(new RoutedEventArgs(StopClickEvent));

            if (IsRunning) StartPulseAnimation();
        }

        // ── Animations ────────────────────────────────────────────────────

        private void StartPulseAnimation()
        {
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.2,
                Duration = new Duration(TimeSpan.FromSeconds(0.8)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            RunningDot.BeginAnimation(Ellipse.OpacityProperty, animation);
        }

        private void StopPulseAnimation()
        {
            RunningDot.BeginAnimation(Ellipse.OpacityProperty, null);
            RunningDot.Opacity = 1.0;
        }
    }
}