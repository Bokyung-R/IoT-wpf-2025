using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMqttSubApp.ViewModels
{
    
    public partial class MainViewModel : ObservableObject
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private string _brokerHost;
        private string _databaseHost;
        
        // 속성 BrokerHost, DatabaseHost
        // 메서드 ConnectBrokerCommand, ConnectDatabaseCommand
        
        public MainViewModel(IDialogCoordinator coordinator) {
            this.dialogCoordinator = coordinator;

            BrokerHost = "210.119.12.74";
            DatabaseHost = "210.119.12.74";
        }
               
        public string BrokerHost
        {
            get => _brokerHost;
            set => SetProperty(ref _brokerHost, value);
        }

        public string DatabaseHost
        {
            get => _databaseHost;
            set => SetProperty(ref _databaseHost, value);
        }

        [RelayCommand]
        public async Task ConnectBroker()
        {
            await this.dialogCoordinator.ShowMessageAsync(this, "브로커연결", "브로커연결합니다");
        }

        [RelayCommand]
        public async Task ConnectDatabase()
        {
            await this.dialogCoordinator.ShowMessageAsync(this, "DB연결", "DB연결합니다");
        }
    }
}
