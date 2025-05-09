﻿using Trolley.Helpers;
using Trolley.Interfaces;
using Trolley.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Trolley.ViewModels
{
    class ManageItemsViewModel : BaseViewModel, ICreatedInventoryItem
    {
        private ObservableCollection<InventoryItem> _items;
        private ObservableCollection<InventoryItem> _filteredItems;
        private int _selectedIndex = 0;
        private InventoryItem _selectedItem;

        private bool _isItemSelected;

        private string _filterText;

        public ManageItemsViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            FilteredItems = Items = new ObservableCollection<InventoryItem>(InventoryItem.LoadItemsNotDeleted());
            IsItemSelected = false;
        }

        public ObservableCollection<InventoryItem> Items
        {
            get { return _items; }
            set { _items = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<InventoryItem> FilteredItems
        {
            get { return _filteredItems; }
            set { _filteredItems = value; NotifyPropertyChanged(); }
        }

        public bool IsItemSelected
        {
            get { return _isItemSelected; }
            set { _isItemSelected = value; NotifyPropertyChanged(); }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; NotifyPropertyChanged(); IsItemSelected = value != -1; }
        }

        public string FilterText
        {
            get { return _filterText; }
            set 
            { 
                _filterText = value;
                if (string.IsNullOrWhiteSpace(_filterText))
                {
                    FilteredItems = Items;
                }
                else
                {
                    FilteredItems = new ObservableCollection<InventoryItem>(Items.Where(x => x.Name.ToLower().Contains(_filterText.ToLower())).ToList());
                }
                NotifyPropertyChanged();
            }
        }

        public InventoryItem SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; NotifyPropertyChanged(); }
        }

        public ICommand MoveToAddItemScreen
        {
            get { return new RelayCommand(LoadAddItemScreen); }
        }

        private void LoadAddItemScreen()
        {
            PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, this) { CurrentUser = CurrentUser });
        }

        public ICommand MoveToEditItemScreen
        {
            get { return new RelayCommand(LoadEditItemScreen); }
        }

        private void LoadEditItemScreen()
        {
            if (SelectedItem != null)
            {
                PushViewModel(new CreateOrEditItemViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
            }
        }

        public ICommand MoveToAdjustQuantityScreen
        {
            get { return new RelayCommand(LoadAdjustQuantityScreen); }
        }

        private void LoadAdjustQuantityScreen()
        {
            PushViewModel(new AdjustQuantityViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public void CreatedInventoryItem(InventoryItem item)
        {
            Items.Add(item);
        }

        public void DeleteItem(InventoryItem item)
        {
            if (item != null)
            {
                item.Delete();
                Items.Remove(item);
            }
        }

        public ICommand MoveToViewQuantityChangesScreen
        {
            get { return new RelayCommand(LoadViewQuantityChangesScreen); }
        }

        private void LoadViewQuantityChangesScreen()
        {
            if (SelectedItem != null)
            {
                PushViewModel(new ViewQuantityAdjustmentsViewModel(ViewModelChanger, SelectedItem) { CurrentUser = CurrentUser });
            }
        }
    }
}
