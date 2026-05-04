using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TaskManagerUI.Controls.Components
{
    public partial class ProgressBar : UserControl
    {
        public ProgressBar()
        {
            InitializeComponent();
            Loaded += (s, e) => AnimateProgress(ProgressWidth);
            SizeChanged += (s, e) => AnimateProgress(ProgressWidth);
        }

        public static readonly DependencyProperty ProgressWidthProperty =
            DependencyProperty.Register(
                "ProgressWidth",
                typeof(double),
                typeof(ProgressBar),
                new PropertyMetadata(0.0, OnProgressChanged));

        public double ProgressWidth
        {
            get => (double)GetValue(ProgressWidthProperty);
            set => SetValue(ProgressWidthProperty, value);
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ProgressBar)d;
            control.AnimateProgress((double)e.NewValue);
        }

        private void AnimateProgress(double percent)
        {
            if (!IsLoaded) return;

            double targetWidth = Root.ActualWidth * (percent / 100.0);

            var animation = new DoubleAnimation
            {
                To = targetWidth,
                Duration = TimeSpan.FromMilliseconds(700),
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            ProgressFill.BeginAnimation(WidthProperty, animation);
        }
    }
}