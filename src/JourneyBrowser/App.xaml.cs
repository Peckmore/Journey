using JourneyBrowser.Views;
using System.Windows;

namespace JourneyBrowser
{
    public partial class App : Application
    {
        #region Methods

        private void AppStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = new MainWindow(Settings.Singleton.HomePage);
            mainWindow.Show();
        }

        #endregion
    }
}