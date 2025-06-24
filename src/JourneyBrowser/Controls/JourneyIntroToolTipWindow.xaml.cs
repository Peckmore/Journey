using System.Windows;

namespace JourneyBrowser.Controls
{
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