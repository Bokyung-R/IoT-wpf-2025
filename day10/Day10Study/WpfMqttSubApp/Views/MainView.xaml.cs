using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfMqttSubApp.ViewModels;

namespace WpfMqttSubApp.Views
{
    /// <summary>
    /// MainView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainView : MetroWindow
    {
        public MainView()
        {
            InitializeComponent();

            var vm = new MainViewModel(DialogCoordinator.Instance);
            this.DataContext = vm;
            vm.PropertyChanged += (sender, e) => {
                if(e.PropertyName == nameof(vm.LogText))
                {
                    // ViewModel의 LogText 속성의 값이 변경되었으면
                    // Dispatcher 객체 내에 UI 렌더링을 넣어줘야 동작함
                    Dispatcher.InvokeAsync(() =>
                    {
                        LogBox.ScrollToEnd();   // 윈앱에서 사용한 적 있음
                    });
                }
            };
        }
    }
}
