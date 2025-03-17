using Trolley.Helpers;
using Trolley.Interfaces;
using Trolley.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Trolley.Models.ShaContext;

namespace Trolley.ViewModels
{
    class ManageAppSettingsViewModel : BaseViewModel
    {
        private uint _autoLogoutLengthMinutes;
        private List<string> _comPort { get; set; }
        private int _cycleTime;
        private ZebraConfig zebraConfig;
        private string _comportSelectedItem;
        private string deviceId;
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
            deviceId = System.Configuration.ConfigurationManager.AppSettings["DEVICE_ID"];

            CycleTime = (System.Configuration.ConfigurationManager.AppSettings["CYCLE_TIME"])!= null ? Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CYCLE_TIME"]) : 0;


            using (var db = new ShaContext())
            {
                zebraConfig = db.ZebraConfigs.Where(x => x.DeviceId == deviceId && x.DeviceType == "Trolley").FirstOrDefault();
                if (zebraConfig != null)
                {
                    if (zebraConfig.CycleTime != CycleTime)
                    {
                        CycleTime = zebraConfig.CycleTime;
                        SaveSetting(key: "CYCLE_TIME", value: CycleTime.ToString());
                    }


                }
                else
                {
                    MessageBox.Show("Device not found in database");
                }
            }



           




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

        public int CycleTime
        {
            get { return _cycleTime; }
            set
            {
                _cycleTime = value;
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
            SaveSetting(key: "CYCLE_TIME", value: CycleTime.ToString());

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
