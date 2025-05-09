﻿using Trolley.Helpers;
using Trolley.Interfaces;
using Trolley.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Trolley.ViewModels
{
    class LoginViewModel : BaseViewModel
    {
        private string _username;
        private SecureString _password;
        private string _error;
        private string _appVersion;
        private string _appDescription;
        private string _appName;

        public LoginViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _appVersion = System.Configuration.ConfigurationManager.AppSettings["APP_VERSION"];
            _appDescription = System.Configuration.ConfigurationManager.AppSettings["APP_DESCRIPTION"];
            _appName = System.Configuration.ConfigurationManager.AppSettings["APP_NAME"] +" "+ _appVersion;
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; NotifyPropertyChanged(); }
        }

        public string AppName
        {
            get { return _appName; }
            set { _appName = value; NotifyPropertyChanged(); }
        }

        public string AppDescription
        {
            get { return _appDescription; }
            set { _appDescription = value; NotifyPropertyChanged(); }
        }

        public string AppVersion
        {
            get { return _appVersion; }
            set { _appVersion = value; NotifyPropertyChanged(); }
        }


        public SecureString Password
        {
            private get { return _password; }
            set { _password = value; NotifyPropertyChanged(); }
        }

        public string Error
        {
            get { return _error; }
            set { _error = value; NotifyPropertyChanged(); }
        }

        public ICommand AttemptLogin
        {
            get { return new RelayCommand(TryLogin); }
        }

        private void TryLogin()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                Error = "Username is required";
            }
            else if (Password == null)
            {
                Error = "Password is required";
            }
            else
            {
                try
                {

                    var user = User.LoadUser(Username, Utilities.SecureStringToString(Password));
                    if (user != null)
                    {
                        Username = "";
                        Password.Clear();
                        Error = "";
                        PushViewModel(new HomeScreenViewModel(ViewModelChanger) { CurrentUser = user });
                    }
                    else
                    {
                        Error = "Invalid username or password";
                    }
                }
                catch (Exception e)
                {
                    Error = "Error: " + e.Message;
                }
            }
        }
    }
}
