using System.Windows;
using System.Windows.Media.Animation;

namespace TaskManagerUI.Helpers
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public GridLength From
        {
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get => (GridLength)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        public IEasingFunction EasingFunction
        {
            get => (IEasingFunction)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }
        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(GridLengthAnimation));

        public override Type TargetPropertyType => typeof(GridLength);

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double from = ((GridLength)GetValue(FromProperty)).Value;
            double to = ((GridLength)GetValue(ToProperty)).Value;
            double progress = animationClock.CurrentProgress ?? 0;

            if (EasingFunction != null)
                progress = EasingFunction.Ease(progress);

            return new GridLength(from + (to - from) * progress);
        }
    }
}