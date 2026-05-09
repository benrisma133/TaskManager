using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TaskManagerUI.Controls.Components
{
    public partial class SidebarToolTip : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string),
                typeof(SidebarToolTip),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d,
                                           DependencyPropertyChangedEventArgs e)
        {
            if (d is SidebarToolTip tip)
                tip.TipText.Text = e.NewValue as string;
        }

        public SidebarToolTip()
        {
            InitializeComponent();

            // Start hidden
            Root.Opacity = 0;
            Translate.X = -6;

            // Fire every time popup becomes visible
            IsVisibleChanged += OnVisibleChanged;
        }

        private void OnVisibleChanged(object sender,
                                       DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                PlayShowAnimation();
            else
                ResetState();
        }

        private void PlayShowAnimation()
        {
            // Fade in
            var fade = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(620),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Slide in from left
            var slide = new DoubleAnimation
            {
                From = -6,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(620),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Root.BeginAnimation(OpacityProperty, fade);
            Translate.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        // Reset so animation replays next time
        private void ResetState()
        {
            Root.BeginAnimation(OpacityProperty, null);
            Translate.BeginAnimation(TranslateTransform.XProperty, null);
            Root.Opacity = 0;
            Translate.X = -6;
        }
    }
}