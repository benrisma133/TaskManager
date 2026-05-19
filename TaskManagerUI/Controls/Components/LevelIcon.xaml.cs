using Service.Enums.Dashboard;
using System.Windows;
using System.Windows.Controls;

namespace TaskManagerUI.Controls.Components
{
    public partial class LevelIcon : UserControl
    {
        // ─── Dependency Property ───────────────────────────────────────
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register(
                nameof(Level),
                typeof(enLevelType),
                typeof(LevelIcon),
                new PropertyMetadata(enLevelType.Beginner, OnLevelChanged)
            );

        public enLevelType Level
        {
            get => (enLevelType)GetValue(LevelProperty);
            set => SetValue(LevelProperty, value);
        }

        private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LevelIcon)d;
            control.UpdateLevel((enLevelType)e.NewValue);
        }

        // ─── Constructor ───────────────────────────────────────────────
        public LevelIcon()
        {
            InitializeComponent();
            //UpdateLevel(Level);
        }

        // ─── Update Visibility Based on Level ─────────────────────────
        private void UpdateLevel(enLevelType level)
        {
            // Hide all
            BeginnerBadge.Visibility = Visibility.Collapsed;
            ExplorerBadge.Visibility = Visibility.Collapsed;
            BuilderBadge.Visibility = Visibility.Collapsed;
            AchieverBadge.Visibility = Visibility.Collapsed;
            ExpertBadge.Visibility = Visibility.Collapsed;
            MasterBadge.Visibility = Visibility.Collapsed;
            LegendBadge.Visibility = Visibility.Collapsed;

            RootGrid.Visibility = Visibility.Visible;

            // Show selected
            switch (level)
            {
                case enLevelType.Beginner:
                    BeginnerBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 58;
                    RootGrid.Height = 58;
                    break;

                case enLevelType.Explorer:
                    ExplorerBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 64;
                    RootGrid.Height = 64;
                    break;

                case enLevelType.Builder:
                    BuilderBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 70;
                    RootGrid.Height = 70;
                    break;

                case enLevelType.Achiever:
                    AchieverBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 76;
                    RootGrid.Height = 76;
                    break;

                case enLevelType.Expert:
                    ExpertBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 82;
                    RootGrid.Height = 96;
                    break;

                case enLevelType.Master:
                    MasterBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 88;
                    RootGrid.Height = 104;
                    break;

                case enLevelType.Legend:
                    LegendBadge.Visibility = Visibility.Visible;
                    RootGrid.Width = 96;
                    RootGrid.Height = 114;
                    break;
            }
        }
    }
}