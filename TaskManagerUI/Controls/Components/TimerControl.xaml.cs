using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace TaskManagerUI.Controls.Components
{
    public partial class TimerControl : UserControl
    {
        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _tickDotTimer;
        private TimeSpan _remaining;
        private bool _completed;
        private bool _tickDotVisible = true;

        public event EventHandler<TimeSpan> Ticked;
        public event EventHandler Completed;
        public event EventHandler Stopped;

        // ── Dependency Property ───────────────────────────────────────────

        public static readonly DependencyProperty EstimatedMinutesProperty =
            DependencyProperty.Register(
                nameof(EstimatedMinutes),
                typeof(int),
                typeof(TimerControl),
                new PropertyMetadata(25, OnEstimatedMinutesChanged));

        public int EstimatedMinutes
        {
            get => (int)GetValue(EstimatedMinutesProperty);
            set => SetValue(EstimatedMinutesProperty, value);
        }

        private static void OnEstimatedMinutesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimerControl)d).Reset();
        }

        // ── Init ──────────────────────────────────────────────────────────

        public TimerControl()
        {
            InitializeComponent();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            _tickDotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _tickDotTimer.Tick += TickDot_Tick;

            Loaded += (s, e) => Reset();
        }

        // ── Timer logic ───────────────────────────────────────────────────

        private void Timer_Tick(object sender, EventArgs e)
        {
            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            UpdateDisplay();
            Ticked?.Invoke(this, _remaining);

            if (_remaining <= TimeSpan.Zero)
                OnCompleted();
        }

        private void TickDot_Tick(object sender, EventArgs e)
        {
            _tickDotVisible = !_tickDotVisible;

            var anim = new DoubleAnimation
            {
                To = _tickDotVisible ? 1.0 : 0.3,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            var scaleAnim = new DoubleAnimation
            {
                To = _tickDotVisible ? 1.0 : 0.6,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            TickDot.BeginAnimation(OpacityProperty, anim);

            var scaleTransform = new ScaleTransform(1, 1, 4, 4);
            TickDot.RenderTransform = scaleTransform;
            TickDot.RenderTransformOrigin = new Point(0.5, 0.5);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void OnCompleted()
        {
            _timer.Stop();
            _tickDotTimer.Stop();
            _completed = true;

            PlayPauseBtn.State = TimerButtonState.Play;

            SetCompletedVisuals();
            PlayFlashAnimation();
            PlayCompletionSound();

            Completed?.Invoke(this, EventArgs.Empty);
        }

        // ── Click handlers ────────────────────────────────────────────────

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_completed) return;

            if (PlayPauseBtn.State == TimerButtonState.Play)
            {
                _timer.Start();
                _tickDotTimer.Start();
                PlayPauseBtn.State = TimerButtonState.Pause;
                StateLabel.Text = "remaining";
                StateLabel.Foreground = TryFindResource("TextSecondaryBrush") as Brush;

                PlayTickTockSound();
            }
            else
            {
                _timer.Stop();
                _tickDotTimer.Stop();
                TickDot.Opacity = 1;
                PlayPauseBtn.State = TimerButtonState.Play;
                StateLabel.Text = "paused";
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _tickDotTimer.Stop();
            Stopped?.Invoke(this, EventArgs.Empty);
            Reset();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void Reset()
        {
            _completed = false;
            _remaining = TimeSpan.FromMinutes(EstimatedMinutes);
            PlayPauseBtn.State = TimerButtonState.Play;

            TickDot.Visibility = Visibility.Visible;
            TickDot.Opacity = 1;
            CheckMark.Visibility = Visibility.Collapsed;

            StateLabel.Text = "remaining";
            StateLabel.Foreground = TryFindResource("TextSecondaryBrush") as Brush;

            ProgressArc.Stroke = TryFindResource("AccentBrush") as Brush;
            TickDot.Fill = TryFindResource("AccentBrush") as Brush;

            UpdateDisplay();
        }

        private void SetCompletedVisuals()
        {
            TickDot.Visibility = Visibility.Collapsed;
            CheckMark.Visibility = Visibility.Visible;

            StateLabel.Text = "done";
            StateLabel.Foreground = TryFindResource("SuccessBrush") as Brush;

            ProgressArc.Stroke = TryFindResource("SuccessBrush") as Brush;

            TimeDisplay.Visibility = Visibility.Collapsed;

            DrawArc(1.0);
        }

        private void UpdateDisplay()
        {
            TimeDisplay.Text = _remaining.ToString(
                _remaining.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss");

            double total = EstimatedMinutes * 60.0;
            double elapsed = total - _remaining.TotalSeconds;
            DrawArc(Math.Clamp(elapsed / total, 0, 1));
        }

        // ── Arc drawing ───────────────────────────────────────────────────

        private void DrawArc(double ratio)
        {
            const double cx = 100, cy = 100, r = 82;
            const double startDeg = -90;

            if (ratio >= 0.9999)
            {
                ProgressArc.Data = new EllipseGeometry(new Point(cx, cy), r, r);
                return;
            }

            if (ratio <= 0.0001)
            {
                ProgressArc.Data = null;
                return;
            }

            double endDeg = startDeg + ratio * 360;
            double startRad = startDeg * Math.PI / 180;
            double endRad = endDeg * Math.PI / 180;

            var start = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
            var end = new Point(cx + r * Math.Cos(endRad), cy + r * Math.Sin(endRad));

            var fig = new PathFigure { StartPoint = start, IsClosed = false };
            fig.Segments.Add(new ArcSegment(end, new Size(r, r), 0,
                (endDeg - startDeg) > 180, SweepDirection.Clockwise, true));

            ProgressArc.Data = new PathGeometry(new[] { fig });
        }

        // ── Effects ───────────────────────────────────────────────────────

        private void PlayFlashAnimation()
        {
            var anim = new ColorAnimation
            {
                To = Colors.Transparent,
                From = Color.FromArgb(40, 29, 158, 117),
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                RepeatBehavior = new RepeatBehavior(2)
            };

            var brush = new SolidColorBrush();
            var overlay = new System.Windows.Shapes.Ellipse
            {
                Width = 164,
                Height = 164,
                Fill = brush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };

            var grid = (Grid)Content;
            var innerGrid = (Grid)grid.Children[0];
            innerGrid.Children.Add(overlay);

            brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }

        private void PlayCompletionSound()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/TaskManagerUI;component/Assets/Sounds/complete.wav");
                var info = Application.GetResourceStream(uri);
                var player = new SoundPlayer(info.Stream);
                player.Play();
            }
            catch { }
        }

        private WaveOutEvent _tickTockOutput;
        private AudioFileReader _tickTockReader;

        private void PlayTickTockSound()
        {
            try
            {
                var path = @"C:\Users\DELL\Desktop\My Prog. Career Path\Projects\C#\WPF\TaskManager\TaskManagerUI\Assets\Sounds\ticktock_pcm.wav";

                _tickTockReader = new AudioFileReader(path);
                _tickTockOutput = new WaveOutEvent();
                _tickTockOutput.Init(_tickTockReader);
                _tickTockOutput.Play();

                var elapsed = 0;
                var fadeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                fadeTimer.Tick += (s, e) =>
                {
                    elapsed += 100;
                    _tickTockReader.Volume = Math.Max(0f, 1f - (elapsed / 3000f));
                    if (elapsed >= 3000)
                    {
                        fadeTimer.Stop();
                        _tickTockOutput.Stop();
                        _tickTockOutput.Dispose();
                        _tickTockReader.Dispose();
                    }
                };
                fadeTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

    }
}