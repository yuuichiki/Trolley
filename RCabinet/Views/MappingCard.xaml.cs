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
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this item?", "Delete Inventory Item", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                (DataContext as ManageItemsViewModel)?.DeleteItem(ItemsGrid.SelectedValue as InventoryItem);
            }
        }


        private void txtCardNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter || txCardNO.Text.Trim() == string.Empty)
            {
                    return;
            }

            if (cbComPort.Text == string.Empty)
            {
                MessageBox.Show("Chưa cài đặt cổng COM, vui lòng vào [cài đặt cổng com] để cài đặt");
                cbComPort.Focus();
                return;
            }
            // call http post request http://172.19.18.35:8103/ETSToEPC/etsCard/nike/{cardNo} to getdata from txtCardNo


        }




        private void MappingCardUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            init();       
        }


        private void init()
        {
            txCardNO.Focus();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cbComPort.Items.Add(port);
            }
            if (cbComPort.Items.Count > 0)
            {
                cbComPort.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Không tìm thấy cổng COM, vui lòng kiểm tra lại");
            }


        }


        private void btReadingEPC_Click(object sender, RoutedEventArgs e)
        {

        }

      


    }
}
