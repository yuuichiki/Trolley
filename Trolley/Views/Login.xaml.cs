﻿using Trolley.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Trolley.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
            Loaded += Login_Loaded;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UsernameInput);
            Loaded -= Login_Loaded;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var dataContext = DataContext as LoginViewModel;
            if (dataContext != null)
            {
                dataContext.Password = PasswordInput.SecurePassword;
            }
        }

        private void UsernameTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.Focus(PasswordInput);
            }
        }

        private void btMinimize_Click(object sender, RoutedEventArgs e)
        {
            //minimum the window
            System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }


        private void btClose_Click(object sender, RoutedEventArgs e)
        {

            //shutdown the application
            System.Windows.Application.Current.Shutdown();
            Environment.Exit(0);
        }



        private void PasswordInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var dataContext = DataContext as LoginViewModel;
                if (dataContext != null)
                {
                    dataContext.AttemptLogin.Execute(null);
                }
            }
        }


    }
}
