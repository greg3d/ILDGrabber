
using System.Windows;
using System.Windows.Input;

namespace DXTesting.ViewModels
{


    class MainWindowViewModel : BaseViewModel
    {

        private string _saveDir;
        private bool _demo;

        Settings settings = Settings.getInstance();

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

        private SaveFormat currentFormat;
        public SaveFormat CurrentFormat
        {
            get { return currentFormat; }
            set { 
                currentFormat = value;
                OnPropertyChanged("CurrentFormat");
            }
        }

        public MainWindowViewModel()
        {
            SaveDir = settings.SaveDir;
            Demo = settings.Demo;
            CurrentFormat = SaveFormat.csv;
        }



    }
}
