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

        #region Methods

        private void Hyperlink_RequestNavigate(object sender, EventArgs e)
        {
            Close();
        }

        #endregion
    }
}