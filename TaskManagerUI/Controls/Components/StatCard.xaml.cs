using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TaskManagerUI.Controls.Components
{
    /// <summary>
    /// Interaction logic for StatCard.xaml
    /// </summary>
    public partial class StatCard : UserControl
    {
        public StatCard()
        {
            InitializeComponent();
        }


        #region Icon Property

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                "Icon",
                typeof(Geometry),
                typeof(StatCard),
                new PropertyMetadata(null));

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion


        #region AccentBrush Property

        public static readonly DependencyProperty AccentBrushProperty =
            DependencyProperty.Register(
                nameof(AccentBrush),
                typeof(Brush),
                typeof(StatCard),
                new PropertyMetadata(null));

        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        #endregion


        #region Title Property
        // 1. Register the DependencyProperty Of Title
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",                 
                typeof(string),            
                typeof(StatCard),         
                new PropertyMetadata("", OnTitleChanged)  
            );

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var badge = (StatCard)d;
            badge.UpdateTitle((string)e.NewValue);
        }

        private void UpdateTitle(string title)
        {
            TitleText.Text = title;
        }

        #endregion


        #region Value Property
        // 1. Register the DependencyProperty Of Value
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(string),
                typeof(StatCard),
                new PropertyMetadata("", OnValueChanged)
            );

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var badge = (StatCard)d;
            badge.UpdateValue((string)e.NewValue);
        }

        private void UpdateValue(string value)
        {
            ValueText.Text = value;
        }

        #endregion


        #region Label Property
        // 1. Register the DependencyProperty Of Label
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(
                "Label",
                typeof(string),
                typeof(StatCard),
                new PropertyMetadata("", OnLabelChanged)
            );

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var badge = (StatCard)d;
            badge.UpdateLabel((string)e.NewValue);
        }

        private void UpdateLabel(string label)
        {
            LabelText.Text = label;
        }

        #endregion

    }
}
