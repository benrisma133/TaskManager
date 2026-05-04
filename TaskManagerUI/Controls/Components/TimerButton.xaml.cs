using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TaskManagerUI.Controls.Components
{
    public partial class TimerButton : UserControl
    {
        private Border _bg;
        public event RoutedEventHandler Click;

        

        public TimerButton()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                UpdateVisual();
            };

            Loaded += TimerButton_Loaded;
        }

        private void TimerButton_Loaded(object sender, RoutedEventArgs e)
        {
            _bg = (Border)Btn.Template.FindName("bg", Btn);
            UpdateVisual();
        }

        public static readonly DependencyProperty BaseBrushProperty =
            DependencyProperty.Register(
                nameof(BaseBrush),
                typeof(Brush),
                typeof(TimerButton),
                new PropertyMetadata(null));

        public Brush BaseBrush
        {
            get => (Brush)GetValue(BaseBrushProperty);
            set => SetValue(BaseBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBrushProperty =
            DependencyProperty.Register(
                nameof(HoverBrush),
                typeof(Brush),
                typeof(TimerButton),
                new PropertyMetadata(null));

        public Brush HoverBrush
        {
            get => (Brush)GetValue(HoverBrushProperty);
            set => SetValue(HoverBrushProperty, value);
        }

        public static readonly DependencyProperty PressedBrushProperty =
            DependencyProperty.Register(
                nameof(PressedBrush),
                typeof(Brush),
                typeof(TimerButton),
                new PropertyMetadata(null));

        public Brush PressedBrush
        {
            get => (Brush)GetValue(PressedBrushProperty);
            set => SetValue(PressedBrushProperty, value);
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State",
                typeof(TimerButtonState),
                typeof(TimerButton),
                new PropertyMetadata(TimerButtonState.Play, OnStateChanged));

        public TimerButtonState State
        {
            get => (TimerButtonState)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                "Icon",
                typeof(Geometry),
                typeof(TimerButton),
                new PropertyMetadata(null));

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Brush ButtonBackground
        {
            get => (Brush)GetValue(ButtonBackgroundProperty);
            set => SetValue(ButtonBackgroundProperty, value);
        }

        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register(nameof(ButtonBackground),
                typeof(Brush),
                typeof(TimerButton),
                new PropertyMetadata(null));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimerButton)d;
            control.UpdateVisual();
        }

        private void UpdateVisual()
        {
            switch (State)
            {
                case TimerButtonState.Play:
                    Icon = TryFindResource("IconPlay") as Geometry;
                    BaseBrush = TryFindResource("TimerPlayBrush") as Brush;
                    HoverBrush = TryFindResource("TimerPlayHoverBrush") as Brush;
                    PressedBrush = TryFindResource("TimerPlayPressedBrush") as Brush;
                    break;

                case TimerButtonState.Pause:
                    Icon = TryFindResource("IconPause") as Geometry;
                    BaseBrush = TryFindResource("TimerPauseBrush") as Brush;
                    HoverBrush = TryFindResource("TimerPauseHoverBrush") as Brush;
                    PressedBrush = TryFindResource("TimerPausePressedBrush") as Brush;
                    break;

                case TimerButtonState.Stop:
                    Icon = TryFindResource("IconStop") as Geometry;
                    BaseBrush = TryFindResource("TimerStopBrush") as Brush;
                    HoverBrush = TryFindResource("TimerStopHoverBrush") as Brush;
                    PressedBrush = TryFindResource("TimerStopPressedBrush") as Brush;
                    break;
            }
        }

        private void AnimateChange()
        {
            var sb = new Storyboard();

            var fade = new DoubleAnimation
            {
                To = 0.3,
                Duration = TimeSpan.FromMilliseconds(80),
                AutoReverse = true
            };

            Storyboard.SetTarget(fade, _bg);
            Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));

            var scaleDown = new DoubleAnimation
            {
                To = 0.92,
                Duration = TimeSpan.FromMilliseconds(80),
                AutoReverse = true
            };

            Storyboard.SetTarget(scaleDown, _bg);
            Storyboard.SetTargetProperty(scaleDown, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            var scaleDownY = new DoubleAnimation
            {
                To = 0.92,
                Duration = TimeSpan.FromMilliseconds(80),
                AutoReverse = true
            };

            Storyboard.SetTarget(scaleDownY, _bg);
            Storyboard.SetTargetProperty(scaleDownY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            sb.Children.Add(fade);
            sb.Children.Add(scaleDown);
            sb.Children.Add(scaleDownY);

            sb.Begin();
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            if (State == TimerButtonState.Stop)
            {
                Stop();
                Click?.Invoke(this,e);
                return;
            }

            AnimateChange();

            //State = State == TimerButtonState.Play
            //    ? TimerButtonState.Pause
            //    : TimerButtonState.Play;

            UpdateVisual();
            Click?.Invoke(this, e);
        }

        public void Stop()
        {
            var sb = new Storyboard();

            var flash = new DoubleAnimation
            {
                To = 0.5,
                Duration = TimeSpan.FromMilliseconds(100),
                AutoReverse = true
            };

            Storyboard.SetTarget(flash, _bg);
            Storyboard.SetTargetProperty(flash, new PropertyPath("Opacity"));

            sb.Children.Add(flash);
            sb.Begin();

            State = TimerButtonState.Stop;
            UpdateVisual();
        }

    }
}