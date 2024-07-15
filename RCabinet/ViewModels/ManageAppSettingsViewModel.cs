using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Properties.Settings.Default.AutoLogoutLength = AutoLogoutLengthMinutes;
            System.Configuration.ConfigurationManager.AppSettings["COMPORT"] = ComPortSelectedItem;
            // pop
            PopToMainMenu();
        }
    }
}
