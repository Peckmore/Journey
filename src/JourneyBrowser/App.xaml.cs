using System.Windows;

namespace JourneyBrowser
{
    public partial class App : Application
    {
        #region Methods

        private void AppStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow(JourneyBrowser.MainWindow.HomePage);
            mainWindow.Show();
        }

        #endregion
    }
}