using Microsoft.Win32;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCabinet.ViewModels
{
    class HomeScreenViewModel : BaseViewModel
    {
        private string deviceId;
        private string deviceType;
        public string DeviceId
        {
            get { return deviceId; }
            set
            {
                deviceId = value;
                NotifyPropertyChanged();
            }
        }
        public string DeviceType
        {
            get { return deviceType; }
            set
            {
                deviceType = value;
                NotifyPropertyChanged();
            }
        }

        public HomeScreenViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {

            deviceId = System.Configuration.ConfigurationManager.AppSettings["DEVICE_ID"];
            deviceType = System.Configuration.ConfigurationManager.AppSettings["DEVICE_TYPE"];
            
        }

        public ICommand MoveToManageItemsScreen
        {
            get { return new RelayCommand(LoadManageItemsScreen); }
        }

        private void LoadManageItemsScreen()
        {
            PushViewModel(new ManageItemsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToManageCurrenciesScreen
        {
            get { return new RelayCommand(LoadManageCurrenciesScreen); }
        }

        private void LoadManageCurrenciesScreen()
        {
            PushViewModel(new ViewCurrenciesViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToScanItemsScreen
        {
            get { return new RelayCommand(LoadScanItemsScreen); }
        }
        public ICommand MoveToMappingCardScreen
        {
            get { return new RelayCommand(LoadMappingCardScreen); }
        }

        public ICommand MoveToMappingUQScreen
        {
            get { return new RelayCommand(LoadMappingUQScreen); }
        }
        

        private void LoadScanItemsScreen()
        {
            PushViewModel(new ScanItemsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        private void LoadMappingCardScreen()
        {
            PushViewModel(new MappingNikeViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }
        private void LoadMappingUQScreen()
        {
            PushViewModel(new MappingUQViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }



        public ICommand MoveToScanAndPurchaseItemsScreen
        {
            get { return new RelayCommand(LoadScanAndPurchaseItemsScreen); }
        }

        private void LoadScanAndPurchaseItemsScreen()
        {
            PushViewModel(new ScanAndPurchaseViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToGenerateBarcodesScreen
        {
            get { return new RelayCommand(LoadGenerateBarcodesScreen); }
        }

        private void LoadGenerateBarcodesScreen()
        {
            PushViewModel(new GenerateBarcodesViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToReportsScreen
        {
            get { return new RelayCommand(LoadReportsScreen); }
        }

        private void LoadReportsScreen()
        {
            PushViewModel(new ViewReportsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToManageItemCategoriesScreen
        {
            get { return new RelayCommand(LoadViewItemTypesScreen); }
        }

        private void LoadViewItemTypesScreen()
        {
            PushViewModel(new ViewItemTypesViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand BackupData
        {
            get { return new RelayCommand(BackupDatabase); }
        }

        private void BackupDatabase()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SIDB file (*.sidb)|*.sidb";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveFileDialog.FileName = "inventory-backup-" + DateTime.Now.ToString("yyyy-MM-dd-H-mm-ss");

            var lastBackupLocation = Properties.Settings.Default.LastBackupFolder;
            if (!string.IsNullOrWhiteSpace(lastBackupLocation) && Directory.Exists(Path.GetDirectoryName(lastBackupLocation)))
            {
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = lastBackupLocation;
            }
            if (saveFileDialog.ShowDialog() == true)
            {
                var dbHelper = new DatabaseHelper();
                File.Copy(dbHelper.GetDatabaseFilePath(), saveFileDialog.FileName);
                Properties.Settings.Default.LastBackupFolder = Path.GetDirectoryName(saveFileDialog.FileName);
            }
        }

        public ICommand Logout
        {
            get { return new RelayCommand(PerformLogout); }
        }
        
        private void PerformLogout()
        {
            PopViewModel();
        }

        public ICommand MoveToManageUsersScreen
        {
            get { return new RelayCommand(LoadManageUsersScreen); }
        }

        private void LoadManageUsersScreen()
        {
            PushViewModel(new ManageUsersViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToChangePasswordScreen
        {
            get { return new RelayCommand(LoadChangePasswordScreen); }
        }

        private void LoadChangePasswordScreen()
        {
            PushViewModel(new ChangePasswordViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToManageAppSettingsScreen
        {
            get { return new RelayCommand(LoadManageAppSettingsScreen); }
        }

        private void LoadManageAppSettingsScreen()
        {
            PushViewModel(new ManageAppSettingsViewModel(ViewModelChanger) { CurrentUser = CurrentUser });
        }
    }
}
