using RCabinet.Models;
using RCabinet.ViewModels;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace RCabinet.Views
{
    /// <summary>
    /// Interaction logic for ManageItems.xaml
    /// </summary>
    public partial class MappingCard : UserControl
    {
        public MappingCard()
        {
            InitializeComponent();
           

        }
        private bool comIsOpened = false;

        private void MappingCard_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(txCardNo);
            Loaded -= MappingCard_Loaded;

        }



        private void txtCardNo_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                (DataContext as MappingCardViewModel)?.LoadCardDataCommand.Execute(null);
            }

        }
         


    }
}
