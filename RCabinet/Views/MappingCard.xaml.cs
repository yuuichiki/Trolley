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
            Keyboard.Focus(txCardId);
            Loaded -= MappingCard_Loaded;
            var viewModel = DataContext as MappingCardViewModel;
            if (viewModel != null)
            {
                viewModel.RequestFocusOnCardId += () =>
                {
                    Keyboard.Focus(txCardId);
                };
            }


        }

        public class ComboBoxSelectionChangedEventArgs : EventArgs
        {
            public string CardNo { get; set; }
            public PosModel SelectedPos { get; set; }
        }
        public event EventHandler<ComboBoxSelectionChangedEventArgs> ComboBoxSelectionChanged;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is CardGridModel cardGridModel)
            {
                var selectedItem = comboBox.SelectedItem as PosModel;

                if (selectedItem != null)
                {
                    // Raise event with CardNo and SelectedPos
                    //ComboBoxSelectionChanged?.Invoke(this, new ComboBoxSelectionChangedEventArgs
                    //{
                    //    CardNo = cardGridModel.CardNo,
                    //    SelectedPos = selectedItem
                    //});


                    var parameter = Tuple.Create(cardGridModel.CardNo,cardGridModel.GangHao,cardGridModel.CustomerColor,cardGridModel.Size, selectedItem);
                 
                    (DataContext as MappingCardViewModel)?.LoadComboBoxCommand.Execute(parameter);


                }
            }

        }

        private void txtCardId_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                (DataContext as MappingCardViewModel)?.LoadCardDataCommand.Execute(null);
            }

        }
    }
}
