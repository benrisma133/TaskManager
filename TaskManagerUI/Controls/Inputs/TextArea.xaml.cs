using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskManagerUI.Controls.Inputs
{
    /// <summary>
    /// Interaction logic for TextArea.xaml
    /// </summary>
    public partial class TextArea : UserControl
    {
        private bool hasInteracted = false;

        public TextArea()
        {
            InitializeComponent();
            InputBox.TextChanged += InputBox_TextChanged;
            UpdateCharacterCount();
        }

        #region Text Property
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextArea),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        public string Text
        {
            get => GetValue(TextProperty) as string ?? string.Empty;
            set => SetValue(TextProperty, value ?? string.Empty);
        }


        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextArea)d;
            string newText = e.NewValue as string ?? string.Empty;


            if (control.InputBox.Text != newText)
            {
                control.InputBox.Text = newText;
            }

            control.UpdateCharacterCount();

            // Only validate if user has interacted
            if (control.hasInteracted)
            {
                control.Validate();
            }
        }
        #endregion

        #region Placeholder Property
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(TextArea),
                new PropertyMetadata("Enter text..."));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region Label Property
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(TextArea),
                new PropertyMetadata("Label"));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
        #endregion

        #region HelperText Property
        public static readonly DependencyProperty HelperTextProperty =
            DependencyProperty.Register("HelperText", typeof(string), typeof(TextArea),
                new PropertyMetadata(string.Empty));

        public string HelperText
        {
            get => (string)GetValue(HelperTextProperty);
            set => SetValue(HelperTextProperty, value);
        }
        #endregion

        #region AreaHeight Property
        public static readonly DependencyProperty AreaHeightProperty =
            DependencyProperty.Register("AreaHeight", typeof(double), typeof(TextArea),
                new PropertyMetadata(120.0));

        public double AreaHeight
        {
            get => (double)GetValue(AreaHeightProperty);
            set => SetValue(AreaHeightProperty, value);
        }
        #endregion

        #region BorderRadius Property
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(TextArea),
                new PropertyMetadata(new CornerRadius(8)));

        public CornerRadius BorderRadius
        {
            get => (CornerRadius)GetValue(BorderRadiusProperty);
            set => SetValue(BorderRadiusProperty, value);
        }
        #endregion

        #region ShowCharacterCount Property
        public static readonly DependencyProperty ShowCharacterCountProperty =
            DependencyProperty.Register("ShowCharacterCount", typeof(bool), typeof(TextArea),
                new PropertyMetadata(true, OnShowCharacterCountChanged));

        public bool ShowCharacterCount
        {
            get => (bool)GetValue(ShowCharacterCountProperty);
            set => SetValue(ShowCharacterCountProperty, value);
        }

        private static void OnShowCharacterCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextArea)d;
            control.UpdateCharacterCount();
        }
        #endregion

        #region Validation Properties
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(TextArea),
                new PropertyMetadata(false, OnValidationRuleChanged));

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register("MinLength", typeof(int), typeof(TextArea),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MinLength
        {
            get => (int)GetValue(MinLengthProperty);
            set => SetValue(MinLengthProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(TextArea),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set
            {
                SetValue(MaxLengthProperty, value);
                InputBox.MaxLength = value > 0 ? value : 0;
                UpdateCharacterCount();
            }
        }

        public static readonly DependencyProperty ValidationMessageTextProperty =
            DependencyProperty.Register("ValidationMessageText", typeof(string), typeof(TextArea),
                new PropertyMetadata(string.Empty));

        public string ValidationMessageText
        {
            get => (string)GetValue(ValidationMessageTextProperty);
            set => SetValue(ValidationMessageTextProperty, value);
        }

        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(TextArea),
                new PropertyMetadata(true));

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }
        #endregion

        #region IsEnabled Override
        public new static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(TextArea),
                new PropertyMetadata(true, OnIsEnabledChanged));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextArea)d;
            control.InputBox.IsEnabled = (bool)e.NewValue;
            control.Opacity = (bool)e.NewValue ? 1.0 : 0.5;
        }
        #endregion

        #region Event Handlers
        private void InputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsRequired)
                hasInteracted = true;
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (hasInteracted)
            {
                Validate();
            }
        }
        #endregion

        #region Validation Logic
        private static void OnValidationRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TextArea)d;
            control.UpdateCharacterCount();

            // Only validate if user has interacted
            if (control.hasInteracted)
            {
                control.Validate();
            }
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            hasInteracted = true;
            Text = InputBox.Text;
            UpdateCharacterCount();
            Validate();
        }

        private void UpdateCharacterCount()
        {
            if (CharCountText == null) return;

            int currentLength = Text?.Length ?? 0;

            if (MaxLength > 0)
            {
                CharCountText.Text = $"{currentLength} / {MaxLength}";

                // Change color when approaching limit
                if (currentLength > MaxLength * 0.9)
                {
                    CharCountText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                }
                else if (currentLength >= MaxLength)
                {
                    CharCountText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }
                else if (!IsValid)
                {
                    CharCountText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                }
                else
                {
                    CharCountText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                }
            }
            else
            {
                CharCountText.Text = $"{currentLength} characters";
            }
        }

        public void Validate()
        {
            // Don't show errors if user hasn't interacted
            if (!hasInteracted)
            {
                return;
            }

            bool valid = true;
            string msg = string.Empty;
            string text = Text ?? string.Empty;

            // Required validation
            if (IsRequired && string.IsNullOrWhiteSpace(text))
            {
                valid = false;
                msg = $"{Label} is required.";
            }
            if (!string.IsNullOrWhiteSpace(text))
            {
                // MinLength validation
                if (MinLength > 0 && text.Length < MinLength)
                {
                    valid = false;
                    msg = $"{Label} must be at least {MinLength} characters.";
                }
                // MaxLength validation
                else if (MaxLength > 0 && text.Length > MaxLength)
                {
                    valid = false;
                    msg = $"{Label} must not exceed {MaxLength} characters.";
                }
            }

            // Update validation state
            bool wasValid = IsValid;
            IsValid = valid;
            ValidationMessageText = msg;

            // Trigger shake animation on validation error
            if (!valid && wasValid)
            {
                TriggerShakeAnimation();
            }

            UpdateCharacterCount();
        }

        /// <summary>
        /// Force validation - useful for form submission
        /// This will validate even if the user hasn't interacted with the field
        /// </summary>
        public bool ValidateForce()
        {
            hasInteracted = true;
            Validate();
            return IsValid;
        }

        /// <summary>
        /// Reset the control to initial state
        /// </summary>
        public void Reset()
        {
            hasInteracted = false;
            Text = string.Empty;
            IsValid = true;
            ValidationMessageText = string.Empty;
            UpdateCharacterCount();
        }

        private void TriggerShakeAnimation()
        {
            if (!hasInteracted)
            {
                var storyboard = (Storyboard)Resources["ShakeAnimation"];
                storyboard.Begin(TextBoxBorder);
            }
        }
        #endregion
    }
}
