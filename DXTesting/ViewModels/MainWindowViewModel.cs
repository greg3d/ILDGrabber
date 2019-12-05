
using System;
using System.Windows.Input;

namespace DXTesting
{

    class MainWindowViewModel : BaseViewModel
    {

        private string _saveDir;
        private bool _demo;
        private SaveFormat _currentFormat;

        private Settings settings = Settings.getInstance();

        public string SaveDir
        {
            get { return _saveDir; }
            set
            {
                if (value == _saveDir) return;
                _saveDir = value;
                settings.SaveDir = value;
                OnPropertyChanged("SaveDir");
            }
        }

        public bool Demo
        {
            get { return _demo; }
            set
            {
                if (!(value ^ _demo)) return;
                _demo = value;
                settings.Demo = value;

                OnPropertyChanged("Demo");
            }
        }


        public SaveFormat CurrentFormat
        {
            get { return _currentFormat; }
            set
            {
                _currentFormat = value;
                OnPropertyChanged("CurrentFormat");
            }
        }

        public ICommand SaveToFileCommand { get; set; }

        public MainWindowViewModel()
        {
            SaveDir = settings.SaveDir;
            Demo = settings.Demo;
            CurrentFormat = SaveFormat.csv;

            SaveToFileCommand = new RelayCommand(SaveToFileMethod, CanExecuteMyMethod);
        }

        private bool CanExecuteMyMethod(object parameter)
        {
            if (Enum.IsDefined(typeof(SaveFormat), CurrentFormat))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SaveToFileMethod(object parameter)
        {
            Connectionz c = Connectionz.getInstance();
            c.SaveAll(CurrentFormat.ToString());
        }


    }
}
