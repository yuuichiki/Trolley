using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCabinet.ViewModels
{
    class BaseViewModel : ChangeNotifier, IChangeViewModel
    {
        IChangeViewModel _viewModelChanger;

        User _currentUser;

        public BaseViewModel(IChangeViewModel viewModelChanger)
        {
            ViewModelChanger = viewModelChanger;
        }

        public IChangeViewModel ViewModelChanger
        {
            get { return _viewModelChanger; }
            set { _viewModelChanger = value; }
        }

        public User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; NotifyPropertyChanged(); }
        }

        #region IChangeViewModel

        public void PopViewModel()
        {
            _viewModelChanger?.PopViewModel();
        }

        public void PushViewModel(BaseViewModel model)
        {
            _viewModelChanger?.PushViewModel(model);
        }

        #endregion
    }
}
