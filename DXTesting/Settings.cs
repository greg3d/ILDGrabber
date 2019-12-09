namespace DXTesting
{
    // setting enums
    public enum SaveFormat { txt, csv, bin }
    public enum MeasRate : int { rate250Hz = 250, rate500Hz = 500, rate1000Hz = 1000, rate2000Hz = 2000 }

    class Settings
    {

        private static Settings instance;

        // поля
        private bool _demo;
        private string _saveDir;
        private MeasRate _fs;

        public MeasRate Fs
        {
            get
            {
                return _fs;
            }
            set
            {
                _fs = value;
            }
        }

        // props
        public bool Demo
        {
            get
            {
                return _demo;
            }
            set
            {
                _demo = value;
            }
        }

        public string SaveDir
        {
            get
            {
                return _saveDir;
            }
            set
            {
                _saveDir = value;
            }
        }

        private Settings()
        {
            _demo = true;
            _saveDir = @"C:\Results";
            _fs = MeasRate.rate250Hz;
        }

        public static Settings getInstance()
        {
            if (instance == null)
            {
                instance = new Settings();
            }

            return instance;
        }

    }
}
