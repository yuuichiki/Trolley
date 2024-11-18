using RCabinet.Helpers;
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
    public partial class MappingCardUQ : UserControl
    {
        public MappingCardUQ()
        {
            InitializeComponent();
        }
        private bool comIsOpened = false;

        private void MappingCardUQ_Loaded(object sender, RoutedEventArgs e)
        {
           var viewModel = DataContext as MappingUQViewModel;
            if (viewModel != null)
            {
                Keyboard.Focus(txtCardId);
            }

        }

       
        private void toppingTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var viewModel = DataContext as MappingUQViewModel;
                if (viewModel != null)
                {
                    if (e.Source is TabControl)
                    {
                        if (tabMappingCard.IsSelected)
                        {
                            viewModel.SelectedTab = "TabMappingCard";
                            viewModel.EnableReadingEPC = false;
                            (DataContext as MappingUQViewModel)?.ChangingTabCommand.Execute(null);

                        }
                        else if (tabCheckingTag.IsSelected)
                        {
                            viewModel.SelectedTab = "TabCheckingTag";
                            viewModel.EnableReadingEPC = true;
                            (DataContext as MappingUQViewModel)?.ChangingTabCommand.Execute(null);
                        }
                    }
                }
            });
        }

        public class ComboBoxSelectionChangedEventArgs : EventArgs
        {
            public string CardNo { get; set; }
            public PosModel SelectedPos { get; set; }
        }
        public event EventHandler<ComboBoxSelectionChangedEventArgs> ComboBoxSelectionChanged;

        private void txtCardId_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                (DataContext as MappingUQViewModel)?.LoadCardDataCommand.Execute(null);
            }

        }

        private void txtTagEPC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (DataContext as MappingUQViewModel)?.CheckingTagDataCommand.Execute(null);

            }
        }

        private void CardGrid_SelectionChanged()
        {

        }
    }
}
