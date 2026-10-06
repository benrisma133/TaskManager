using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace TaskManagerUI.Controls.Components
{
    public partial class TimerControl : UserControl
    {
        // ============================
        // FIELDS — visual state only, no time-tracking
        // ============================
        private readonly DispatcherTimer _tickDotTimer;
        private bool _tickDotVisible = true;
        private bool _isRunning = false;
        private bool _isCompleted = false;

        // ============================
        // DEPENDENCY PROPERTY
        // ============================
        public static readonly DependencyProperty EstimatedMinutesProperty =
            DependencyProperty.Register(
                nameof(EstimatedMinutes),
                typeof(int),
                typeof(TimerControl),
                new PropertyMetadata(25));

        public int EstimatedMinutes
        {
            get => (int)GetValue(EstimatedMinutesProperty);
            set => SetValue(EstimatedMinutesProperty, value);
        }
        // No OnChanged handler: the control never resets itself.
        // The page always follows an EstimatedMinutes change with SetRemaining(...).

        // ============================
        // EVENTS — pure user intent, no guarantee of effect
        // ============================
        public event EventHandler? PlayRequested;
        public event EventHandler? PauseRequested;
        public event EventHandler? StopRequested;

        // ============================
        // CONSTRUCTOR
        // ============================
        public TimerControl()
        {
            InitializeComponent();

            _tickDotTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _tickDotTimer.Tick += TickDot_Tick;

            Unloaded += (s, e) => _tickDotTimer.Stop();
        }

        // ============================
        // TICK DOT — purely decorative pulse, independent of real elapsed time
        // ============================
        private void TickDot_Tick(object? sender, EventArgs e)
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

        // ============================
        // BUTTON CLICKS — raise intent only. No state mutation here.
        // ============================
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isCompleted) return;

            if (!_isRunning)
            {
                PlayRequested?.Invoke(this, EventArgs.Empty);
                PlayTickTockSound();
            }

            else
                PauseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            if (_isCompleted) return;
            StopRequested?.Invoke(this, EventArgs.Empty);
        }

        // ============================
        // PUBLIC — called by the page once it has decided what happened
        // ============================
        public void SetRunning(bool running)
        {
            _isRunning = running;
            _isCompleted = false;

            PlayPauseBtn.State = running ? TimerButtonState.Pause : TimerButtonState.Play;
            StateLabel.Text = running ? "remaining" : "paused";
            StateLabel.Foreground = TryFindResource("TextSecondaryBrush") as Brush;

            if (running)
            {
                _tickDotTimer.Start();
                //PlayTickTockSound();
            }
            else
            {
                _tickDotTimer.Stop();
                TickDot.Opacity = 1;
            }
        }

        public void SetIdle()
        {
            _isRunning = false;
            _isCompleted = false;
            _tickDotTimer.Stop();

            PlayPauseBtn.State = TimerButtonState.Play;
            StateLabel.Text = "remaining";
            StateLabel.Foreground = TryFindResource("TextSecondaryBrush") as Brush; // رجوع لون النص للوضع العادي

            TickDot.Visibility = Visibility.Visible;
            TickDot.Opacity = 1;
            CheckMark.Visibility = Visibility.Collapsed;         // إخفاء علامة الصح
            TimeDisplay.Visibility = Visibility.Visible;         // إظهار الأرقام الفعالة

            // إرجاع الإطار والنقطة للألوان الافتراضية (AccentBrush) وحيد اللون الأخضر
            ProgressArc.Stroke = TryFindResource("AccentBrush") as Brush;
            TickDot.Fill = TryFindResource("AccentBrush") as Brush;

            // تنظيف داك الـ Overlay المؤقت اللي دار ليه PlayFlashAnimation باش يختفي اللون الأخضر العالق
            try
            {
                var grid = (Grid)Content;
                if (grid.Children.Count > 0 && grid.Children[0] is Grid innerGrid)
                {
                    // قلب على أي Ellipse مؤقت بحجم 164 وحذفو من الواجهة
                    var overlaysToRemove = innerGrid.Children.OfType<System.Windows.Shapes.Ellipse>()
                        .Where(e => e.IsHitTestVisible == false && e.Width == 164).ToList();

                    foreach (var overlay in overlaysToRemove)
                    {
                        innerGrid.Children.Remove(overlay);
                    }
                }
            }
            catch { }

            // ملاحظة: هنا ما كنقيسوش DrawArc، داكشي غيتكلف بيه _InitTimer و SetRemaining 
            // باش يرسم النسبة الحقيقية الجديدة (بحال 50% إيلا كانت دقيقة دازت ودقيقة تزادت) بشكل صحيح 100%!
        }

        public void SetCompleted()
        {
            _isRunning = false;
            _isCompleted = true;
            _tickDotTimer.Stop();

            PlayPauseBtn.State = TimerButtonState.Play;
            TickDot.Visibility = Visibility.Collapsed;
            CheckMark.Visibility = Visibility.Visible;
            StateLabel.Text = "done";
            StateLabel.Foreground = TryFindResource("SuccessBrush") as Brush;
            ProgressArc.Stroke = TryFindResource("SuccessBrush") as Brush;
            TimeDisplay.Visibility = Visibility.Collapsed;

            DrawArc(1.0);
            PlayFlashAnimation();
            PlayCompletionSound();
        }

        // ============================
        // DISPLAY — no side effects, no GlobalTimer involvement whatsoever
        // ============================
        public void SetRemaining(TimeSpan remaining)
        {
            if (remaining < TimeSpan.Zero)
                remaining = TimeSpan.Zero;

            TimeDisplay.Text = remaining.ToString(
                remaining.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss");

            double total = EstimatedMinutes * 60.0;
            double elapsed = total - remaining.TotalSeconds;
            DrawArc(total > 0 ? Math.Clamp(elapsed / total, 0, 1) : 0);
        }

        // ============================
        // DRAW ARC (unchanged)
        // ============================
        public void DrawArc(double ratio)
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

        // ============================
        // SOUNDS / FLASH (unchanged, just no longer self-triggered from a click)
        // ============================
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
                var player = new System.Media.SoundPlayer(info.Stream);
                player.Play();
            }
            catch { }
        }

        private NAudio.Wave.WaveOutEvent? _tickTockOutput;
        private NAudio.Wave.AudioFileReader? _tickTockReader;

        private void PlayTickTockSound()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/TaskManagerUI;component/Assets/Sounds/ticktock_pcm.wav");
                var info = Application.GetResourceStream(uri);

                _tickTockOutput?.Stop();
                _tickTockOutput?.Dispose();
                _tickTockReader?.Dispose();

                _tickTockReader = new NAudio.Wave.AudioFileReader(
                    CopyStreamToTempFile(info.Stream)); // see note below
                _tickTockOutput = new NAudio.Wave.WaveOutEvent();
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
            catch { /* non-critical sound, fail silently */ }
        }

        private static string CopyStreamToTempFile(Stream stream)
        {
            var path = Path.Combine(Path.GetTempPath(), "ticktock_pcm.wav");
            using var file = File.Create(path);
            stream.CopyTo(file);
            return path;
        }
    }
}