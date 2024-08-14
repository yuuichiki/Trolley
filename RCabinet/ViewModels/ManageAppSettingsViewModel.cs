using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RCabinet.ViewModels
{
    class ManageAppSettingsViewModel : BaseViewModel
    {
        private uint _autoLogoutLengthMinutes;
        private List<string> _comPort { get; set; }
        private string _comportSelectedItem;
        public ManageAppSettingsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _autoLogoutLengthMinutes = Properties.Settings.Default.AutoLogoutLength;
            ComPort = new List<string>();
            ComPort.Add("Select Comport");
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                ComPort.Add(port);
            }
            ComPort.Sort();
            ComPortSelectedItem = System.Configuration.ConfigurationManager.AppSettings["COMPORT"];
        }

        #region Properties

        public uint AutoLogoutLengthMinutes
        {
            get { return _autoLogoutLengthMinutes; }
            set { _autoLogoutLengthMinutes = value; NotifyPropertyChanged(); }
        }
 
        public List<string> ComPort
        {
            get { return _comPort; }
            set { _comPort = value; NotifyPropertyChanged(); }
        }
        public string ComPortSelectedItem
        {
            get { return _comportSelectedItem; }
            set
            {
                _comportSelectedItem = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public ICommand ReturnToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public ICommand SaveAppSettings
        {
            get { return new RelayCommand(SaveAndPop); }
        }

        private void SaveAndPop()
        {
            // save
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = config.AppSettings.Settings;
            Properties.Settings.Default.AutoLogoutLength = AutoLogoutLengthMinutes;
            Properties.Settings.Default.Save();
            SaveSetting(key: "COMPORT", value: ComPortSelectedItem);
            

            // pop
            PopToMainMenu();
        }

        public void SaveSetting(string key, string value)
        {
            // Open the configuration file for the application.
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Modify the appSettings section.
            var settings = config.AppSettings.Settings;

            if (settings[key] == null)
            {
                // Add a new key-value pair.
                settings.Add(key, value);
            }
            else
            {
                // Update the value of an existing key.
                settings[key].Value = value;
            }

            // Save the configuration file.
            config.Save(ConfigurationSaveMode.Modified);

            // Refresh the appSettings section.
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
