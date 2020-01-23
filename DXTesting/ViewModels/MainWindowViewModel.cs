
using System;
using System.Windows;
using System.Windows.Input;

namespace DXTesting
{

    class MainWindowViewModel : BaseViewModel
    {
        private bool _demo;
        private SaveFormat _currentFormat;
        private Visibility _progBarVisibility = Visibility.Hidden;

        private Settings settings = Settings.getInstance();

       
        public Visibility ProgBarVisibility
        {
            get
            {
                return _progBarVisibility;
            }
            set
            {
                _progBarVisibility = value;
                OnPropertyChanged("ProgBarVisibility");
            }
        }


        public bool Demo
        {
            get { return _demo; }
            set
            {
                if (!(value ^ _demo))
                {
                    return;
                }

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



        public MainWindowViewModel()
        {
            Demo = settings.Demo;
            CurrentFormat = SaveFormat.csv;
            SaveToFileCommand = new RelayCommand(SaveToFileMethod, CanExecuteMyMethod);
        }

        public ICommand SaveToFileCommand { get; set; }

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
