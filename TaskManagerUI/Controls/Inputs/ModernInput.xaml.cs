using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskManagerUI.Controls.Inputs
{
    /// <summary>
    /// Interaction logic for ModernInput.xaml
    /// </summary>
    public partial class ModernInput : UserControl
    {
        private bool hasInteracted = false;

        private Func<string, string> _externalValidator;


        public ModernInput()
        {
            InitializeComponent();
            IsValid = true;

        }

        #region Dependency Properties

        #region Height Property
        public static readonly DependencyProperty InputHeightProperty =
            DependencyProperty.Register(
                "InputHeight",
                typeof(double),
                typeof(ModernInput),
                new PropertyMetadata(40.0, OnInputHeightChanged)
            );

        public double InputHeight
        {
            get => (double)GetValue(InputHeightProperty);
            set => SetValue(InputHeightProperty, value);
        }

        private static void OnInputHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            double newHeight = (double)e.NewValue;
            control.TextBoxBorder.Height = newHeight;
        }
        #endregion

        #region Label Property
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(ModernInput),
                new PropertyMetadata("Label", OnLabelChanged));

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            control.LabelText.Text = e.NewValue?.ToString() ?? "Label";
        }
        #endregion

        #region Placeholder Property
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(ModernInput),
                new PropertyMetadata("Enter text...", OnPlaceholderChanged));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            control.PlaceholderText.Text = e.NewValue?.ToString() ?? "";
        }
        #endregion

        #region Text Property
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ModernInput),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            string newValue = e.NewValue?.ToString() ?? "";

            if (control.InputBox.Text != newValue)
            {
                control.InputBox.Text = newValue;
            }

            // Only validate if user has interacted
            if (control.hasInteracted)
            {
                control.Validate(live: true);
            }
        }
        #endregion

        #region Border Properties
        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(ModernInput),
                new PropertyMetadata(new CornerRadius(8), OnBorderRadiusChanged));

        public CornerRadius BorderRadius
        {
            get => (CornerRadius)GetValue(BorderRadiusProperty);
            set => SetValue(BorderRadiusProperty, value);
        }

        private static void OnBorderRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            control.TextBoxBorder.CornerRadius = (CornerRadius)e.NewValue;
        }
        #endregion

        #region IsValid and ValidationMessage Properties
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(ModernInput),
                new PropertyMetadata(true));

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public static readonly DependencyProperty ValidationMessageTextProperty =
            DependencyProperty.Register("ValidationMessageText", typeof(string), typeof(ModernInput),
                new PropertyMetadata("This field is required.", OnValidationMessageChanged));

        public string ValidationMessageText
        {
            get => (string)GetValue(ValidationMessageTextProperty);
            set => SetValue(ValidationMessageTextProperty, value);
        }

        private static void OnValidationMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            control.ValidationMessage.Text = e.NewValue?.ToString() ?? "";
        }
        #endregion

        #region Validation Rule Properties
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(ModernInput),
                new PropertyMetadata(false, OnValidationRuleChanged));

        public bool IsRequired
        {
            get => (bool)GetValue(IsRequiredProperty);
            set => SetValue(IsRequiredProperty, value);
        }

        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register("MinLength", typeof(int), typeof(ModernInput),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MinLength
        {
            get => (int)GetValue(MinLengthProperty);
            set => SetValue(MinLengthProperty, value);
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength", typeof(int), typeof(ModernInput),
                new PropertyMetadata(0, OnValidationRuleChanged));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set
            {
                SetValue(MaxLengthProperty, value);
                InputBox.MaxLength = value > 0 ? value : 0;
            }
        }
        #endregion

        #region ValidationType Property
        public static readonly DependencyProperty ValidationTypeProperty =
            DependencyProperty.Register("ValidationType", typeof(ValidationType), typeof(ModernInput),
                new PropertyMetadata(ValidationType.None, OnValidationRuleChanged));

        public ValidationType ValidationType
        {
            get => (ValidationType)GetValue(ValidationTypeProperty);
            set => SetValue(ValidationTypeProperty, value);
        }
        #endregion

        #region IsEnabled Property
        public new static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ModernInput),
                new PropertyMetadata(true, OnIsEnabledChanged));

        public new bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            control.InputBox.IsEnabled = (bool)e.NewValue;
            control.Opacity = (bool)e.NewValue ? 1.0 : 0.5;
        }

        private static void OnValidationRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ModernInput)d;
            // Only validate if user has already interacted
            if (control.hasInteracted)
            {
                control.Validate();
            }
        }
        #endregion

        #endregion

        #region Event Handlers

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mark that user has started interacting
            hasInteracted = true;

            Text = InputBox.Text;

            // Live validation while typing
            Validate(live: true);

            TextChanged?.Invoke(this, e);
        }

        private void InputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Mark interaction when user focuses the field
            hasInteracted = true;
        }

        private void InputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Full validation when user leaves the field
            if (hasInteracted)
            {
                Validate();
            }
        }

        #endregion

        #region Validation

        public void Validate(bool live = false, Func<string, string> externalValidator = null)
        {
            // 🔥 STORE external validator
            if (externalValidator != null)
                _externalValidator = externalValidator;

            // Don't show validation errors if user hasn't interacted yet
            if (!hasInteracted && !live)
            {
                return;
            }

            string text = Text ?? string.Empty;
            bool wasValid = IsValid;

            IsValid = true;
            ValidationMessageText = string.Empty;

            // Required validation
            if (IsRequired && string.IsNullOrWhiteSpace(text))
            {
                IsValid = false;
                ValidationMessageText = $"{Label} is required.";
                return;
            }

            // Skip other validations if empty and not required
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // MinLength
            if (MinLength > 0 && text.Length < MinLength)
            {
                IsValid = false;
                ValidationMessageText = $"{Label} must be at least {MinLength} characters.";
                return;
            }

            // MaxLength
            if (MaxLength > 0 && text.Length > MaxLength)
            {
                IsValid = false;
                ValidationMessageText = $"{Label} must not exceed {MaxLength} characters.";
                return;
            }

            // Type validation
            switch (ValidationType)
            {
                case ValidationType.Email:
                    if (!IsValidEmail(text))
                    {
                        IsValid = false;
                        ValidationMessageText = "Please enter a valid email address.";
                    }
                    break;
                case ValidationType.Numeric:
                    if (!IsNumeric(text))
                    {
                        IsValid = false;
                        ValidationMessageText = "Please enter only numbers.";
                    }
                    break;
                case ValidationType.AlphaNumeric:
                    if (!IsAlphaNumeric(text))
                    {
                        IsValid = false;
                        ValidationMessageText = "Please enter only letters and numbers.";
                    }
                    break;
                case ValidationType.Name:
                    if (!IsValidName(text))
                    {
                        IsValid = false;
                        ValidationMessageText = "Please enter a valid name (letters and spaces only).";
                    }
                    break;
            }

            // ✅ USE STORED external validator
            if (_externalValidator != null)
            {
                string error = _externalValidator(text);
                if (!string.IsNullOrEmpty(error))
                {
                    IsValid = false;
                    ValidationMessageText = error;
                }
            }
        }

        /// <summary>
        /// Force validation - useful for form submission
        /// This will validate even if the user hasn't interacted with the field
        /// </summary>
        public bool ValidateForce(Func<string, string> externalValidator = null)
        {
            hasInteracted = true;
            Validate(live: false, externalValidator: externalValidator);
            return IsValid;
        }

        /// <summary>
        /// Set external validation error (e.g., from server)
        /// </summary>
        public void SetExternalError(string message)
        {
            hasInteracted = true;
            IsValid = false;
            ValidationMessageText = message;
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
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsNumeric(string text)
        {
            return double.TryParse(text, out _);
        }

        private bool IsAlphaNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c))
                    return false;
            }
            return true;
        }

        private bool IsValidName(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c) && c != '-' && c != '\'')
                    return false;
            }
            return true;
        }

        #endregion

        public event TextChangedEventHandler TextChanged;


    }

    #region Enums

    public enum ValidationType
    {
        None,
        Email,
        Numeric,
        AlphaNumeric,
        Name
    }

    #endregion
}
