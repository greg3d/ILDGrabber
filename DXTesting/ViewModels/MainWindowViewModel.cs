using System;

namespace DXTesting.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        private string _saveDir = @"C:\Results";
        public string SaveDir
        {
            get { return _saveDir; }
            set
            {
                if (value == _saveDir) return;
                _saveDir = value;
                OnPropertyChanged("SaveDir");
            }
        }
    }
}
