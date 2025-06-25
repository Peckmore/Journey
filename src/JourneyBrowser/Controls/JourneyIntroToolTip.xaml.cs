using System.Windows;

namespace JourneyBrowser.Controls
{
    internal partial class JourneyIntroToolTip : Window
    {
        #region Construction

        public JourneyIntroToolTip()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void Hyperlink_RequestNavigate(object sender, EventArgs e)
        {
            // There is only one link in our text, and when the user clicks it we close the window.
            Close();
        }

        #endregion
    }
}