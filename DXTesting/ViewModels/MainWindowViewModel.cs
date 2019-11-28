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

        public MainWindowViewModel()
        {

            SaveDir = settings.SaveDir;
            Demo = settings.Demo;
        }


    }
}
