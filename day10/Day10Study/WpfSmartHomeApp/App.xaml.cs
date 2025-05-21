using System.Windows;
using WpfSmartHomeApp.Helpers;
using WpfSmartHomeApp.ViewModels;
using WpfSmartHomeApp.Views;

namespace WpfSmartHomeApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var viewModel = new MainViewModel();
            var view = new MainWindow
            {
                DataContext = viewModel,
            };
            view.ShowDialog();
            Common.LOGGER.Info("스마트홈 모니터링앱 실행");
        }
    }

}
