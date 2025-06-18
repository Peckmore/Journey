using System.Windows;

namespace JourneyBrowser
{
    /// <summary>
    /// Interaction logic for ToolTipWindow.xaml
    /// </summary>
    public partial class JourneyIntroToolTipWindow : Window
    {
        #region Construction

        public JourneyIntroToolTipWindow()
        {
            InitializeComponent();
        }

        #endregion


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
