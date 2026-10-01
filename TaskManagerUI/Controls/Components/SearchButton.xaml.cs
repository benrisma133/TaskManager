using System.Windows;
using System.Windows.Controls;

namespace TaskManagerUI.Controls.Components
{
    public partial class SearchButton : UserControl
    {
        public SearchButton()
        {
            InitializeComponent();
        }

        // ============================
        // EXPOSE BUTTON CLICK EVENT
        // ============================
        public event RoutedEventHandler Click
        {
            add { SearchBtn.Click += value; }
            remove { SearchBtn.Click -= value; }
        }

        // ============================
        // EXPOSE ENABLED STATE
        // ============================
        new public bool IsEnabled
        {
            get => SearchBtn.IsEnabled;
            set => SearchBtn.IsEnabled = value;
        }
    }
}